using LMS.Helpers;
using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.Bank;
using LMS.Models.ViewModels.StudentService;
using LMS.Repositories;
using LMS.Services.Interfaces.StudentService;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace LMS.Services.Impl.StudentService;
public class PaymentService : IPaymentService
{
    private readonly IGenericRepository<Payment, Guid> _payRepo;
    private readonly IGenericRepository<ClassRegistration, long> _regRepo;
    private readonly IGenericRepository<Class, Guid> _classRepo;
    private readonly VnPayOptions _opt;

    public PaymentService(
        IGenericRepository<Payment, Guid> payRepo,
        IGenericRepository<ClassRegistration, long> regRepo,
        IGenericRepository<Class, Guid> classRepo,
        IOptions<VnPayOptions> opt)
    {
        _payRepo = payRepo;
        _regRepo = regRepo;
        _classRepo = classRepo;
        _opt = opt.Value;
    }

    public async Task<PaymentCheckoutVm> CreateRegistrationPaymentAsync(
        Guid studentId, Guid classId, string paymentMethod, CancellationToken ct = default)
    {
        var reg = await _regRepo.FirstOrDefaultAsync(
                    r => r.StudentId == studentId && r.ClassId == classId
                                                  && r.RegistrationStatus == "approved",
                            asNoTracking: false,
                            includes: new Expression<Func<ClassRegistration, object>>[]
                                        { r => r.Class! },
                            ct: ct);

        if (reg is null || reg.Class is null)
            throw new InvalidOperationException("Registration not found or not approved.");

        var cls = reg.Class;
        var amount = (cls.UnitPrice ?? 0m) * (cls.TotalSessions ?? 0);

        var payment = new Payment
        {
            StudentId = studentId,
            ClassId = classId,
            RegistrationId = reg.RegistrationId,
            Amount = amount,
            PaymentMethod = paymentMethod,  //VNPAY
            PaymentStatus = "pending",
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow
        };

        await _payRepo.AddAsync(payment, saveNow: false, ct);
        await _payRepo.SaveChangesAsync(ct);

        return new PaymentCheckoutVm(
                    payment.PaymentId, payment.RegistrationId, classId, amount,
                    payment.PaymentMethod, payment.PaymentStatus,
                    payment.CreatedAt, payment.PaidAt, cls.ClassName);
    }

    public async Task<PagedResult<PaymentHistoryVm>> ListMyPaymentsAsync(
        Guid studentId, DateOnly? from = null, DateOnly? to = null, string? status = null,
        int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;
        var skip = (pageIndex - 1) * pageSize;

        var pays = await _payRepo.ListAsync(
            p => p.StudentId == studentId
                 && (!from.HasValue || p.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue))
                 && (!to.HasValue || p.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue))
                 && (string.IsNullOrEmpty(status) || p.PaymentStatus == status),
            orderBy: q => q.OrderByDescending(p => p.CreatedAt),
            skip: skip, take: pageSize,
            asNoTracking: true,
            includes: new Expression<Func<Payment, object>>[]
                        { p => p.Class! },
            ct: ct);

        var total = await _payRepo.CountAsync(
            p => p.StudentId == studentId
                 && (!from.HasValue || p.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue))
                 && (!to.HasValue || p.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue))
                 && (string.IsNullOrEmpty(status) || p.PaymentStatus == status), ct);

        var items = pays.Select(p => new PaymentHistoryVm(
            p.PaymentId, p.Amount, p.PaymentMethod, p.PaymentStatus, p.CreatedAt, p.PaidAt, p.Class?.ClassName ?? ""))
            .ToList();

        return new PagedResult<PaymentHistoryVm>(items, total, pageIndex, pageSize);
    }

    public async Task<bool> UpdateRegistrationPaymentAsync(
                                Guid paymentId, string newStatus,
                                decimal? amountVerified = null, DateTime? paidAt = null,
                                CancellationToken ct = default)
    {
        var payment = await _payRepo.GetByIdAsync(paymentId, asNoTracking: false, ct);
        if (payment is null) return false;

        // Idempotent: nếu đã paid thì bỏ qua
        if (string.Equals(payment.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase)) return true;

        // Nếu cổng trả về amount, đối chiếu để an toàn
        if (amountVerified.HasValue && amountVerified.Value != payment.Amount)
        {
            payment.PaymentStatus = "failed"; // sai số tiền → fail
            await _payRepo.UpdateAsync(payment, saveNow: false, ct);
            await _payRepo.SaveChangesAsync(ct);
            return false;
        }

        // Cập nhật trạng thái theo IPN/Webhook
        payment.PaymentStatus = newStatus; // "paid" | "failed" | "canceled"...
        if (string.Equals(newStatus, "paid", StringComparison.OrdinalIgnoreCase))
        {
            payment.PaidAt = paidAt ?? DateTime.UtcNow;
        }

        await _payRepo.UpdateAsync(payment, saveNow: false, ct);
        await _payRepo.SaveChangesAsync(ct);
        return true;
    }

    // helper
    public Guid ParsePaymentIdFromTxnRef(string txnRef)
    {
        // Dùng PaymentId dưới dạng Guid "N" làm vnp_TxnRef
        return Guid.ParseExact(txnRef, "N");
    }

    //  VNPAY
    public string BuildPaymentUrl(Guid paymentId, decimal amountVnd, string orderInfo, string clientIp)
    {
        var nowGmt7 = DateTime.UtcNow.AddHours(7);
        var lib = new VnPayLibrary();

        lib.AddRequestData("vnp_Version", _opt.Version);
        lib.AddRequestData("vnp_Command", _opt.Command);
        lib.AddRequestData("vnp_TmnCode", _opt.TmnCode);
        lib.AddRequestData("vnp_Amount", ((long)(amountVnd * 100m)).ToString()); // *100
        lib.AddRequestData("vnp_CreateDate", nowGmt7.ToString("yyyyMMddHHmmss"));
        lib.AddRequestData("vnp_ExpireDate", nowGmt7.AddMinutes(15).ToString("yyyyMMddHHmmss"));
        lib.AddRequestData("vnp_CurrCode", _opt.CurrCode);
        lib.AddRequestData("vnp_IpAddr", clientIp);
        lib.AddRequestData("vnp_Locale", _opt.Locale);
        lib.AddRequestData("vnp_OrderInfo", orderInfo);
        lib.AddRequestData("vnp_OrderType", "other");
        lib.AddRequestData("vnp_ReturnUrl", _opt.ReturnUrl);
        lib.AddRequestData("vnp_TxnRef", paymentId.ToString("N"));

        // Tạo URL + ký HMACSHA512 (nội bộ lib sẽ thực hiện)
        return lib.CreateRequestUrl(_opt.PaymentUrl, _opt.HashSecret);
    }

    public bool TryParseAndVerify(IQueryCollection query, out VnPayReturn data)
    {
        var lib = new VnPayLibrary();
        foreach (var (k, v) in query)
        {
            if (!string.IsNullOrEmpty(k) && k.StartsWith("vnp_"))
                lib.AddResponseData(k, v!);
        }
        var hash = query["vnp_SecureHash"].ToString();
        var ok = lib.ValidateSignature(hash, _opt.HashSecret);  // verify HMAC
        var refStr = lib.GetResponseData("vnp_TxnRef");
        var amountStr = lib.GetResponseData("vnp_Amount");      // *100
        var rsp = lib.GetResponseData("vnp_ResponseCode");
        var bank = lib.GetResponseData("vnp_BankCode");
        var txnNo = lib.GetResponseData("vnp_TransactionNo");

        var amount = decimal.TryParse(amountStr, out var a) ? a / 100m : 0m;
        var pid = Guid.TryParseExact(refStr, "N", out var g) ? g : Guid.Empty;

        data = new VnPayReturn(pid, amount, rsp, bank, txnNo);
        return ok;
    }


    public async Task<(Guid PaymentId, string VnpTxnRef, string Url)> BeginVnpayCheckoutAsync(
       Guid studentId, Guid classId, string bankCode, string? description, CancellationToken ct = default)
    {
        // 1) Đảm bảo đã có registration "approved"
        var reg = await _regRepo.FirstOrDefaultAsync(
            r => r.StudentId == studentId && r.ClassId == classId && r.RegistrationStatus == "approved",
            asNoTracking: false,
            includes: new System.Linq.Expressions.Expression<Func<ClassRegistration, object>>[] { r => r.Class! },
            ct: ct);
        if (reg is null || reg.Class is null)
            throw new InvalidOperationException("Registration not found or not approved.");

        // 2) Tính amount (UnitPrice * TotalSessions)
        var cls = reg.Class;
        var amount = (cls.UnitPrice ?? 0m) * (cls.TotalSessions ?? 0);

        // 3) Tạo txnRef (key giao dịch ở cổng; lưu vào DB để đối chiếu)
        var vnpTxnRef = Guid.NewGuid().ToString("N");

        // 4) Tạo Payment "PENDING"
        var p = new Payment
        {
            StudentId = studentId,
            ClassId = classId,
            RegistrationId = reg.RegistrationId,
            Amount = amount,
            PaymentMethod = "VNPAY",
            PaymentStatus = "PENDING",
            CreatedAt = DateTime.UtcNow,
            PaidAt = null,
            VnpTxnRef = vnpTxnRef,      // ⚠️ cần cột này trong DB/Entity
            BankCode = null,           // sẽ cập nhật lúc callback
        };
        await _payRepo.AddAsync(p, saveNow: false, ct);
        await _payRepo.SaveChangesAsync(ct);

        // 5) Tạo URL thanh toán (giao cho Page gọi VNPAY client cụ thể)
        // Ở Application layer tránh phụ thuộc IVnpayClient. Trả về "placeholder",
        // Razor Page sẽ tạo URL thật dựa vào vnpTxnRef + amount.
        var url = $"__VNPAY_URL_PLACEHOLDER__?vnp_TxnRef={vnpTxnRef}&amount={(int)(amount * 100)}&bank={bankCode}";
        return (p.PaymentId, vnpTxnRef, url);
    }

    // CALLBACK XÁC NHẬN
    public async Task<PaymentReceiptVm> ConfirmVnpayAsync(
        string vnpTxnRef, string rspCode, decimal amountVnd, string? bankCode, CancellationToken ct = default)
    {
        var p = await _payRepo.FirstOrDefaultAsync(x => x.VnpTxnRef == vnpTxnRef,
            asNoTracking: false,
            includes: new System.Linq.Expressions.Expression<Func<Payment, object>>[] { x => x.Class! }, ct: ct);

        if (p is null)
            throw new InvalidOperationException($"Payment not found for vnp_TxnRef={vnpTxnRef}");

        if (p.PaymentStatus == "PAID")
        {
            return new PaymentReceiptVm(p.PaymentId, p.PaymentStatus, p.Amount, p.CreatedAt, p.PaidAt, p.BankCode, p.Class?.ClassName ?? "");
        }

        if (rspCode == "00")
        {
            // Nếu DB đã có Amount, đối chiếu; nếu 0 thì nhận theo gateway
            var finalAmount = p.Amount == 0 ? amountVnd : p.Amount;
            if (p.Amount != 0 && p.Amount != amountVnd)
            {
                p.PaymentStatus = "FAILED";
                await _payRepo.UpdateAsync(p, saveNow: false, ct);
                await _payRepo.SaveChangesAsync(ct);
                return new PaymentReceiptVm(p.PaymentId, p.PaymentStatus, p.Amount, p.CreatedAt, p.PaidAt, p.BankCode, p.Class?.ClassName ?? "");
            }

            p.PaymentStatus = "PAID";
            p.PaidAt = DateTime.UtcNow;
            p.BankCode = bankCode;
            p.Amount = finalAmount;

            await _payRepo.UpdateAsync(p, saveNow: false, ct);
            await _payRepo.SaveChangesAsync(ct);
        }
        else
        {
            p.PaymentStatus = "FAILED";
            await _payRepo.UpdateAsync(p, saveNow: false, ct);
            await _payRepo.SaveChangesAsync(ct);
        }

        return new PaymentReceiptVm(p.PaymentId, p.PaymentStatus, p.Amount, p.CreatedAt, p.PaidAt, p.BankCode, p.Class?.ClassName ?? "");
    }
}

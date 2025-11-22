using System.Globalization;
using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VNPAY;
using VNPAY.Models.Exceptions;

namespace LMS.Pages.Student;

public class VnpayCallbackModel : PageModel
{
    private readonly IVnpayClient _vnpay;
    private readonly CenterDbContext _db;

    public VnpayCallbackModel(IVnpayClient vnpay, CenterDbContext db)
    {
        _vnpay = vnpay;
        _db = db;
    }

    // View data
    public bool IsSuccess { get; private set; }
    public string Title { get; private set; } = "";
    public string Message { get; private set; } = "";

    public string? VnpTxnRef { get; private set; }
    public string? BankCode { get; private set; }
    public string? CardType { get; private set; }
    public string? BankTranNo { get; private set; }
    public string? OrderInfo { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime? PayDateLocal { get; private set; }
    public Guid? PaymentId { get; private set; }
    public string RawQuery { get; private set; } = "";

    public async Task OnGetAsync()
    {
        // Xác thực chữ ký (nếu sai sẽ ném lỗi). Không dừng trang để vẫn hiển thị thông tin.
        try { _ = _vnpay.GetPaymentResult(Request); }
        catch (VnpayException) { /* giữ im lặng cho UX, log nếu cần */ }

        var q = Request.Query;

        var rspCode = q["vnp_ResponseCode"].ToString();
        VnpTxnRef = q["vnp_TxnRef"].ToString();
        BankCode = q["vnp_BankCode"].ToString();
        CardType = q["vnp_CardType"].ToString();
        BankTranNo = q["vnp_BankTranNo"].ToString();
        OrderInfo = q["vnp_OrderInfo"].ToString();
        var amountStr = q["vnp_Amount"].ToString();        // đơn vị = VND * 100
        var payDateStr = q["vnp_PayDate"].ToString();       // yyyyMMddHHmmss

        if (decimal.TryParse(amountStr, out var vnPayAmount))
            Amount = vnPayAmount / 100m;

        if (DateTime.TryParseExact(payDateStr, "yyyyMMddHHmmss",
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var payUtc))
        {
            // VNPAY trả UTC; hiển thị theo local server
            PayDateLocal = DateTime.SpecifyKind(payUtc, DateTimeKind.Utc).ToLocalTime();
        }

        // Tìm bản ghi Payment theo TxnRef
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.VnpTxnRef == VnpTxnRef);
        if (payment is null)
        {
            IsSuccess = false;
            Title = "Không tìm thấy giao dịch";
            Message = $"Không tìm thấy Payment với mã {VnpTxnRef}.";
            RawQuery = Request.QueryString.Value ?? "";
            return;
        }

        PaymentId = payment.PaymentId;

        // Idempotent
        if (payment.PaymentStatus == "PAID")
        {
            IsSuccess = true;
            Title = "Giao dịch đã xác nhận";
            Message = "Giao dịch này đã được xác nhận trước đó.";
            RawQuery = Request.QueryString.Value ?? "";
            return;
        }

        if (rspCode == "00")
        {
            if (payment.Amount != 0 && payment.Amount != Amount)
            {
                IsSuccess = false;
                Title = "Sai số tiền";
                Message = $"Số tiền không khớp (DB={payment.Amount:N0} VND, VNPAY={Amount:N0} VND).";
            }
            else
            {
                payment.PaymentStatus = "PAID";
                payment.PaidAt = DateTime.UtcNow;
                payment.BankCode = BankCode;
                if (payment.Amount == 0) payment.Amount = Amount;
                await _db.SaveChangesAsync();

                IsSuccess = true;
                Title = "Thanh toán thành công";
                Message = "Cảm ơn bạn! Giao dịch đã được xác nhận.";
            }
        }
        else
        {
            payment.PaymentStatus = "FAILED";
            await _db.SaveChangesAsync();

            IsSuccess = false;
            Title = "Thanh toán chưa thành công";
            Message = MapResponse(rspCode);
        }

        RawQuery = Request.QueryString.Value ?? "";
    }

    private static string MapResponse(string code) => code switch
    {
        "00" => "Giao dịch thành công.",
        "07" => "Giao dịch bị nghi ngờ gian lận.",
        "09" => "Thẻ/Tài khoản chưa đăng ký InternetBanking.",
        "10" => "Xác thực thông tin thẻ/tài khoản không đúng.",
        "24" => "Khách hàng hủy giao dịch.",
        _ => $"Mã phản hồi: {code}."
    };
}

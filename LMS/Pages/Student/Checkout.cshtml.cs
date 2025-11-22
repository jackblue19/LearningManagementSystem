using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VNPAY;
using LMS.Data;
using LMS.Models.Entities;
using LMS.Pages._Base;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Student;

[Authorize(Roles = "student")]
public class CheckoutModel : AppPageModel
{
    private readonly IVnpayClient _vnpay;
    private readonly CenterDbContext _db;

    public CheckoutModel(IVnpayClient vnpay, CenterDbContext db)
    { _vnpay = vnpay; _db = db; }

    [BindProperty(SupportsGet = true)] public long? RegistrationId { get; set; }
    [BindProperty(SupportsGet = true)] public decimal? Amount { get; set; } // cho phép lấy từ route
    [BindProperty(SupportsGet = true)] public string? Desc { get; set; }

    // hiển thị
    public string ClassName { get; private set; } = "";

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        // Nếu có registrationId mà chưa có amount thì tự tính
        if (RegistrationId.HasValue && Amount is null)
        {
            var reg = await _db.ClassRegistrations.AsNoTracking()
                        .FirstOrDefaultAsync(r => r.RegistrationId == RegistrationId.Value
                                               && r.StudentId == CurrentUserId, ct);
            if (reg is null) return NotFound();

            var cls = await _db.Classes.AsNoTracking().FirstOrDefaultAsync(c => c.ClassId == reg.ClassId, ct);
            if (cls is null) return NotFound();

            var unit = cls.UnitPrice ?? 0m;
            var sessions = cls.TotalSessions ?? 0;
            Amount = unit * sessions;
            ClassName = cls.ClassName ?? "";
            Desc ??= $"Học phí lớp {ClassName}";
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(decimal amount, string description, long? registrationId, CancellationToken ct)
    {
        var studentId = CurrentUserId;

        var info = _vnpay.CreatePaymentUrl((int)amount, description, VNPAY.Models.Enums.BankCode.ANY);
        var paymentUrl = info.Url;

        var uri = new Uri(paymentUrl);
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var txnRef = query.TryGetValue("vnp_TxnRef", out var v) ? v.ToString() : Guid.NewGuid().ToString("N");

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            VnpTxnRef = txnRef,
            Amount = amount,
            PaymentStatus = "PENDING",
            PaidAt = null,
            StudentId = studentId,
            PaymentMethod = "VNPAY",
            // Nếu bảng Payments có RegistrationId: RegistrationId = registrationId
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);

        return Redirect(paymentUrl);
    }
}

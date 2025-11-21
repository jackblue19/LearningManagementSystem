using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using VNPAY;
using VNPAY.Models.Exceptions;
using LMS.Data;

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

    public string Status { get; private set; } = "PENDING";
    public string? BankCode { get; private set; }
    public string TxnRef { get; private set; } = "";
    public decimal Amount { get; private set; }
    public string ClassName { get; private set; } = "";
    public string SubjectName { get; private set; } = "";
    public DateTime? PaidAt { get; private set; }
    public string RawQuery { get; private set; } = "";

    public async Task OnGetAsync()
    {
        try { var _ = _vnpay.GetPaymentResult(Request); } catch (VnpayException) { }

        var rspCode = Request.Query["vnp_ResponseCode"].ToString();
        TxnRef = Request.Query["vnp_TxnRef"].ToString();
        BankCode = Request.Query["vnp_BankCode"].ToString();
        var amountStr = Request.Query["vnp_Amount"].ToString();
        var amountVnd = string.IsNullOrWhiteSpace(amountStr) ? 0m : decimal.Parse(amountStr) / 100m;
        RawQuery = Request.QueryString.Value ?? "";

        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.VnpTxnRef == TxnRef);
        if (payment is null) { Status = "NOT_FOUND"; return; }

        if (payment.PaymentStatus == "PAID")
        {
            Status = "PAID";
        }
        else if (rspCode == "00")
        {
            if (payment.Amount != 0 && payment.Amount != amountVnd)
            {
                payment.PaymentStatus = "FAILED";
                await _db.SaveChangesAsync();
                Status = "FAILED";
            }
            else
            {
                payment.PaymentStatus = "PAID";
                payment.PaidAt = DateTime.UtcNow;
                payment.BankCode = BankCode;
                if (payment.Amount == 0) payment.Amount = amountVnd;
                await _db.SaveChangesAsync();
                Status = "PAID";
            }
        }
        else
        {
            payment.PaymentStatus = "FAILED";
            await _db.SaveChangesAsync();
            Status = "FAILED";
        }

        Amount = payment.Amount;
        PaidAt = payment.PaidAt;

        // Thêm hiển thị lớp & môn (cần Payment.ClassId)
        if (payment.ClassId != Guid.Empty)
        {
            var cls = await _db.Classes.Include(c => c.Subject)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(c => c.ClassId == payment.ClassId);
            ClassName = cls?.ClassName ?? "";
            SubjectName = cls?.Subject?.SubjectName ?? "";
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VNPAY.Models.Enums;
using VNPAY;
using LMS.Data;
using Microsoft.AspNetCore.WebUtilities;
using LMS.Models.Entities;

namespace LMS.Pages.Student.temp;
public class CheckoutModel : PageModel
{
    private readonly IVnpayClient _vnpay;
    private readonly CenterDbContext _db;

    [BindProperty] public decimal Amount { get; set; }
    [BindProperty] public string Description { get; set; } = string.Empty;

    public CheckoutModel(IVnpayClient vnpay, CenterDbContext db)
    {
        _vnpay = vnpay;
        _db = db;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        //
        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            Username = "jackblue",
            Email = $"jackblue@gmail.com",
            PasswordHash = "123@123",
            FullName = "jack blue",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        _db.Users.Add(newUser);
        await _db.SaveChangesAsync();
        var userId = newUser.UserId;

        var info = _vnpay.CreatePaymentUrl((int)Amount, Description, BankCode.ANY);
        var paymentUrl = info.Url;

        var uri = new Uri(paymentUrl);
        var qs = QueryHelpers.ParseQuery(uri.Query);
        var txnRef = qs.TryGetValue("vnp_TxnRef", out var v) ? v.ToString() : Guid.NewGuid().ToString("N");

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            VnpTxnRef = txnRef,
            Amount = Amount,
            PaymentStatus = "PENDING",
            PaidAt = DateTime.UtcNow,
            StudentId = userId
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return Redirect(paymentUrl);
    }
}

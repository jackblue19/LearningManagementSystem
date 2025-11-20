using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Models.ViewModels.Bank;
using LMS.Services.Interfaces.StudentService;
using LMS.Models.ViewModels;

namespace LMS.Pages.Student.temp;

public class PaymentsModel : PageModel
{
    private readonly IPaymentService _paymentSvc;

    public PaymentsModel(IPaymentService paymentSvc) => _paymentSvc = paymentSvc;

    [BindProperty(SupportsGet = true)]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? To { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public PagedResult<PaymentHistoryVm>? Page { get; set; }
    public PaymentCheckoutVm? LastPayment { get; set; }
    public string? Flash { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Page = await _paymentSvc.ListMyPaymentsAsync(StudentId, From, To, Status, PageIndex, PageSize, ct);
        Flash = TempData["flash"] as string;
    }

    public async Task<IActionResult> OnPostPayAsync(Guid classId, string method, CancellationToken ct)
    {
        LastPayment = await _paymentSvc.CreateRegistrationPaymentAsync(StudentId, classId, method, ct);
        TempData["flash"] = $"Thanh toán thành công: {LastPayment.Amount:0.##} cho lớp {LastPayment.ClassName}.";
        return RedirectToPage(new
        {
            studentId = StudentId,
            from = From?.ToString("yyyy-MM-dd"),
            to = To?.ToString("yyyy-MM-dd"),
            status = Status,
            pageIndex = PageIndex,
            pageSize = PageSize
        });
    }
}

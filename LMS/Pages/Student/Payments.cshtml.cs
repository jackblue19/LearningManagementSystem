//using System.ComponentModel.DataAnnotations;
//using LMS.Models.Entities;
//using LMS.Services.Interfaces.StudentService;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace LMS.Pages.Student;

//public class PaymentsModel : PageModel
//{
//    private readonly IPaymentService _paymentService;

//    public PaymentsModel(IPaymentService paymentService)
//    {
//        _paymentService = paymentService;
//    }

//    // -----------------------------
//    // Form + Query properties
//    // -----------------------------
//    [BindProperty(SupportsGet = true)]
//    [Display(Name = "Student Id")]
//    public Guid StudentId { get; set; }

//    [BindProperty]
//    [Display(Name = "Class Id")]
//    public Guid ClassId { get; set; }

//    [BindProperty]
//    [DataType(DataType.Currency)]
//    public decimal Amount { get; set; }

//    [BindProperty]
//    [Display(Name = "Payment Method")]
//    public string? PaymentMethod { get; set; }

//    // -----------------------------
//    // View Data
//    // -----------------------------
//    public IReadOnlyList<Payment> Payments { get; private set; } = Array.Empty<Payment>();

//    [TempData] public string? StatusMessage { get; set; }
//    [TempData] public string? ErrorMessage { get; set; }

//    // -----------------------------
//    // Load payments on GET
//    // -----------------------------
//    public async Task OnGetAsync(CancellationToken ct)
//    {
//        await LoadPaymentsAsync(ct);
//    }

//    // -----------------------------
//    // Create a payment
//    // -----------------------------
//    public async Task<IActionResult> OnPostCreateAsync(CancellationToken ct)
//    {
//        if (StudentId == Guid.Empty || ClassId == Guid.Empty)
//        {
//            ModelState.AddModelError(string.Empty, "Student Id and Class Id are required.");
//            await LoadPaymentsAsync(ct);
//            return Page();
//        }

//        if (Amount <= 0)
//        {
//            ModelState.AddModelError(nameof(Amount), "Amount must be greater than 0.");
//            await LoadPaymentsAsync(ct);
//            return Page();
//        }

//        try
//        {
//            var payment = await _paymentService.CreatePaymentAsync(
//                StudentId,
//                ClassId,
//                Amount,
//                PaymentMethod,
//                ct
//            );

//            if (payment is null)
//            {
//                ErrorMessage = "Student is not registered for this class or registration is inactive.";
//            }
//            else
//            {
//                StatusMessage = $"Payment recorded successfully ({payment.Amount:C}).";
//            }
//        }
//        catch (Exception ex)
//        {
//            ErrorMessage = ex.Message;
//        }

//        return RedirectToPage(new { StudentId });
//    }

//    // -----------------------------
//    // Helper to load payments safely
//    // -----------------------------
//    private async Task LoadPaymentsAsync(CancellationToken ct)
//    {
//        if (StudentId == Guid.Empty)
//        {
//            Payments = Array.Empty<Payment>();
//            return;
//        }

//        Payments = await _paymentService.GetStudentPaymentsAsync(StudentId, ct);
//    }
//}

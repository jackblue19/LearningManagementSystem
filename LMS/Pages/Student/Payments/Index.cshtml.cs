using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Pages._Base;
using LMS.Services.Student;
using LMS.Models.ViewModels;
using LMS.Models.Entities;

namespace LMS.Pages.Student.Payments;

[Authorize(Roles = "student")]
public sealed class IndexModel : AppPageModel
{
    private readonly IStudentPaymentService _svc;
    private readonly IStudentBillingService _billing;
    public IndexModel(IStudentPaymentService svc, IStudentBillingService billing)
    { _svc = svc; _billing = billing; }

    // Filters lịch sử
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? From { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? To { get; set; }
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int Size { get; set; } = 10;

    public PagedResult<Payment> Data { get; private set; } = new(Array.Empty<Payment>(), 0, 1, 10);

    // NEW: danh sách cần thanh toán
    public IReadOnlyList<PayableDto> Payables { get; private set; } = Array.Empty<PayableDto>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        // payables (đưa lên đầu trang)
        Payables = await _billing.ListPayablesAsync(CurrentUserId, ct);

        // history
        DateTime? fromUtc = From.HasValue ? DateTime.SpecifyKind(From.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc) : null;
        DateTime? toUtc = To.HasValue ? DateTime.SpecifyKind(To.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc) : null;

        Data = await _svc.PagedPaymentsAsync(
            studentId: CurrentUserId,
            status: Status,
            fromUtc: fromUtc,
            toUtc: toUtc,
            page: Page,
            size: Size,
            ct: ct);
    }
}

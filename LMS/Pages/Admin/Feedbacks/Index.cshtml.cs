using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Admin.Feedbacks;

public class IndexModel : PageModel
{
    private readonly IFeedbackService _feedbackService;

    public IndexModel(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? VisibilityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public FeedbackListViewModel ViewModel { get; set; } = null!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var result = await _feedbackService.GetFeedbacksAsync(
            searchTerm: SearchTerm,
            statusFilter: StatusFilter,
            visibilityFilter: VisibilityFilter,
            pageIndex: PageIndex,
            pageSize: 20,
            ct: ct);

        ViewModel = new FeedbackListViewModel
        {
            Feedbacks = result,
            SearchTerm = SearchTerm,
            StatusFilter = StatusFilter,
            VisibilityFilter = VisibilityFilter
        };
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(long id, string status, CancellationToken ct = default)
    {
        try
        {
            await _feedbackService.UpdateStatusAsync(id, status, ct);
            TempData["SuccessMessage"] = "Feedback status updated.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to update status: {ex.Message}";
        }

        return RedirectToPage(new { SearchTerm, StatusFilter, VisibilityFilter, PageIndex });
    }

    public async Task<IActionResult> OnPostUpdateVisibilityAsync(long id, bool isVisible, CancellationToken ct = default)
    {
        try
        {
            await _feedbackService.UpdateVisibilityAsync(id, isVisible, ct);
            TempData["SuccessMessage"] = "Feedback visibility updated.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to update visibility: {ex.Message}";
        }

        return RedirectToPage(new { SearchTerm, StatusFilter, VisibilityFilter, PageIndex });
    }
}


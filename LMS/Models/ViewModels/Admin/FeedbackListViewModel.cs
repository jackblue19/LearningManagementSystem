using System.Collections.Generic;
using LMS.Models.ViewModels;

namespace LMS.Models.ViewModels.Admin;

public class FeedbackListViewModel
{
    public PagedResult<FeedbackListItemViewModel> Feedbacks { get; set; } = null!;
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }
    public bool? VisibilityFilter { get; set; }
    public IReadOnlyList<string> StatusOptions { get; set; } = new List<string> { "pending", "approved", "rejected" };
}


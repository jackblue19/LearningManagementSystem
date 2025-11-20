using LMS.Models.ViewModels;

namespace LMS.Models.ViewModels.Admin;

public class AuditLogListViewModel
{
    public PagedResult<AuditLogListItemViewModel> Logs { get; set; } = null!;
    public string? SearchTerm { get; set; }
    public string? ActionFilter { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public IReadOnlyList<string> ActionOptions { get; set; } = Array.Empty<string>();
}


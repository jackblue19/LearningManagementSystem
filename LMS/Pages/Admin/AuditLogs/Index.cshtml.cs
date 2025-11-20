using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Admin.AuditLogs;

public class IndexModel : PageModel
{
    private readonly IAuditLogService _auditLogService;

    public IndexModel(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ActionFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? DateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? DateTo { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public AuditLogListViewModel ViewModel { get; set; } = null!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var result = await _auditLogService.GetAuditLogsAsync(
            searchTerm: SearchTerm,
            actionFilter: ActionFilter,
            dateFrom: DateFrom,
            dateTo: DateTo,
            pageIndex: PageIndex,
            pageSize: 25,
            ct: ct);

        var actionOptions = result.Items
            .Select(l => l.ActionType)
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(a => a)
            .ToList();

        ViewModel = new AuditLogListViewModel
        {
            Logs = result,
            SearchTerm = SearchTerm,
            ActionFilter = ActionFilter,
            DateFrom = DateFrom,
            DateTo = DateTo,
            ActionOptions = actionOptions
        };
    }
}


using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Admin;

public class AdminDashboardModel : PageModel
{
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardModel(IAdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public AdminDashboardViewModel Dashboard { get; set; } = null!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        Dashboard = await _dashboardService.GetDashboardAsync(ct);
    }
}

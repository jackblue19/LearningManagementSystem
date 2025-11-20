using LMS.Repositories.Interfaces.Info;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

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
[Authorize(Policy = "AdminOnly")]
public class AdminDashboardModel : PageModel
{
    private readonly IUserRepository _userRepo;
    private readonly IAuthService _authService;

    public AdminDashboardModel(IUserRepository userRepo, IAuthService authService)
    {
        _userRepo = userRepo;
        _authService = authService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user needs to setup password (Google OAuth users)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            var user = await _userRepo.GetByIdAsync(userGuid);
            if (user != null && _authService.IsOAuthTempPassword(user.PasswordHash))
            {
                return RedirectToPage("/Common/SetupPassword");
            }
        }

        return Page();
    }
}

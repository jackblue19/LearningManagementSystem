using LMS.Repositories.Interfaces.Info;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authorization;
using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace LMS.Pages.Admin;

//[Authorize(Policy = "AdminOnly")]
public class AdminDashboardModel : PageModel
{
    private readonly IUserRepository _userRepo;
    private readonly IAuthService _authService;
    private readonly IAdminDashboardService _dashboardService;

    public AdminDashboardModel(IUserRepository userRepo,
                                IAuthService authService,
                                IAdminDashboardService dashboardService)
    {
        _userRepo = userRepo;
        _authService = authService;
        _dashboardService = dashboardService;
    }

    public AdminDashboardViewModel Dashboard { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync()
    {
        Dashboard = await _dashboardService.GetDashboardAsync();
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

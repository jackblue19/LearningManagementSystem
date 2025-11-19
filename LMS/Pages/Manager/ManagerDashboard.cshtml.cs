using LMS.Repositories.Interfaces.Info;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class ManagerDashboardModel : PageModel
{
    private readonly IUserRepository _userRepo;

    public ManagerDashboardModel(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if user needs to setup password (Google OAuth users)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
        {
            var user = await _userRepo.GetByIdAsync(userGuid);
            if (user != null && user.PasswordHash.Length > 50)
            {
                return RedirectToPage("/Common/SetupPassword");
            }
        }

        return Page();
    }
}

using LMS.Models.ViewModels.Auth;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using LMS.Repositories.Interfaces.Info;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using LMS.Models.Entities;

namespace LMS.Pages.Common;

[Authorize]
public class SetupPasswordModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepo;

    public SetupPasswordModel(IAuthService authService, IUserRepository userRepo)
    {
        _authService = authService;
        _userRepo = userRepo;
    }

    [BindProperty]
    public SetupPasswordViewModel Input { get; set; } = new();

    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"[SetupPassword] UserId from claims: {userId}");
        
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            Console.WriteLine($"[SetupPassword] Invalid userId, redirecting to login");
            return RedirectToPage("/Common/Login");
        }

        var user = await _userRepo.GetByIdAsync(userGuid);
        if (user == null)
        {
            Console.WriteLine($"[SetupPassword] User not found in database");
            return RedirectToPage("/Common/Login");
        }
        
        Console.WriteLine($"[SetupPassword] User found - Email: {user.Email}, Role: {user.RoleDesc}");

        // Check if user needs to setup password (Google OAuth users)
        var needsPasswordSetup = _authService.IsOAuthTempPassword(user.PasswordHash);
        Console.WriteLine($"[SetupPassword] Needs password setup: {needsPasswordSetup}");
        
        if (!needsPasswordSetup)
        {
            // User already has a proper password, redirect to dashboard
            Console.WriteLine($"[SetupPassword] User has proper password, redirecting to dashboard");
            return RedirectToRoleDashboard(user.RoleDesc);
        }

        Email = user.Email;
        FullName = user.FullName ?? user.Username;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return RedirectToPage("/Common/Login");
        }

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
            
            // Reload user data
            var currentUser = await _userRepo.GetByIdAsync(userGuid);
            if (currentUser != null)
            {
                Email = currentUser.Email;
                FullName = currentUser.FullName ?? currentUser.Username;
            }
            
            return Page();
        }

        var user = await _userRepo.GetByIdAsync(userGuid, asNoTracking: false);
        if (user == null)
        {
            return RedirectToPage("/Common/Login");
        }

        // Update password and phone
        user.PasswordHash = _authService.HashPassword(Input.Password);
        if (!string.IsNullOrEmpty(Input.Phone))
        {
            user.Phone = Input.Phone.Trim();
        }
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user, saveNow: true);

        // Refresh authentication with updated user info
        await RefreshSignInAsync(user);

        TempData["SuccessMessage"] = "Đặt mật khẩu thành công! Bây giờ bạn có thể đăng nhập bằng email và mật khẩu này.";

        // Redirect to appropriate dashboard based on user's role
        return RedirectToRoleDashboard(user.RoleDesc);
    }

    private async Task RefreshSignInAsync(User user)
    {
        // Create updated claims with correct role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleDesc ?? "student"),
            new Claim("FullName", user.FullName ?? user.Username),
            new Claim("Avatar", user.Avatar ?? "/images/default-avatar.png")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        // Sign in again with updated claims
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    private IActionResult RedirectToRoleDashboard(string? role)
    {
        return role?.ToLower() switch
        {
            "admin" => RedirectToPage("/Admin/AdminDashboard"),
            "manager" => RedirectToPage("/Manager/ManagerDashboard"),
            "teacher" => RedirectToPage("/Teacher/TeacherDashboard"),
            "student" => RedirectToPage("/Student/StudentDashboard"),
            _ => RedirectToPage("/Student/StudentDashboard")
        };
    }
}

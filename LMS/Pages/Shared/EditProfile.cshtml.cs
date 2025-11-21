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

namespace LMS.Pages.Shared;

[Authorize]
public class EditProfileModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepo;
    private readonly LMS.Services.Interfaces.AdminService.IAuditLogService _auditLogService;

    public EditProfileModel(
        IAuthService authService, 
        IUserRepository userRepo,
        LMS.Services.Interfaces.AdminService.IAuditLogService auditLogService)
    {
        _authService = authService;
        _userRepo = userRepo;
        _auditLogService = auditLogService;
    }

    [BindProperty]
    public EditProfileViewModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return RedirectToPage("/Common/Login");
        }

        var user = await _userRepo.GetByIdAsync(userGuid);
        if (user == null)
        {
            return RedirectToPage("/Common/Login");
        }

        Input = new EditProfileViewModel
        {
            UserId = user.UserId,
            FullName = user.FullName ?? "",
            Email = user.Email,
            Phone = user.Phone,
            CurrentAvatar = user.Avatar,
            CurrentCoverImage = user.CoverImageUrl
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
        {
            return RedirectToPage("/Common/Login");
        }

        Input.UserId = userGuid;

        // Get old data for audit log
        var oldUser = await _userRepo.GetByIdAsync(userGuid);
        var oldData = oldUser != null ? 
            $"{{ \"FullName\": \"{oldUser.FullName}\", \"Phone\": \"{oldUser.Phone}\" }}" : null;

        var (success, user, errorMessage) = await _authService.UpdateProfileAsync(Input);

        if (!success)
        {
            ErrorMessage = errorMessage ?? "Cập nhật thông tin thất bại.";
            return Page();
        }

        SuccessMessage = "Cập nhật thông tin thành công!";
        
        // Reload user data and refresh authentication
        if (user != null)
        {
            Input.CurrentAvatar = user.Avatar;
            Input.CurrentCoverImage = user.CoverImageUrl;
            
            // Refresh authentication claims with updated info
            await RefreshSignInAsync(user);

            // Log Audit
            await _auditLogService.LogActionAsync(
                userId: user.UserId,
                actionType: "Update Profile",
                entityName: "User",
                recordId: user.UserId.ToString(),
                oldData: oldData,
                newData: $"{{ \"FullName\": \"{user.FullName}\", \"Phone\": \"{user.Phone}\" }}"
            );
        }

        return Page();
    }

    private async Task RefreshSignInAsync(User user)
    {
        // Create updated claims with current user info
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
        
        // Preserve existing auth properties
        var existingPrincipal = User;
        var isPersistent = existingPrincipal.Identity?.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme;
        
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        // Sign in again with updated claims
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}

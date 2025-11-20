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

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class EditProfileModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepo;

    public EditProfileModel(IAuthService authService, IUserRepository userRepo)
    {
        _authService = authService;
        _userRepo = userRepo;
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

        var (success, user, errorMessage) = await _authService.UpdateProfileAsync(Input);

        if (!success)
        {
            ErrorMessage = errorMessage ?? "Cập nhật thông tin thất bại.";
            return Page();
        }

        SuccessMessage = "Cập nhật thông tin thành công!";
        
        if (user != null)
        {
            Input.CurrentAvatar = user.Avatar;
            Input.CurrentCoverImage = user.CoverImageUrl;
            await RefreshSignInAsync(user);
        }

        return Page();
    }

    private async Task RefreshSignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleDesc ?? "manager"),
            new Claim("FullName", user.FullName ?? user.Username),
            new Claim("Avatar", user.Avatar ?? "/images/default-avatar.png")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}

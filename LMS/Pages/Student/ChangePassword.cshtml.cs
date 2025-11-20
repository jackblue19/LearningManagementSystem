using LMS.Models.ViewModels.Auth;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LMS.Pages.Student;

[Authorize(Policy = "StudentOnly")]
public class ChangePasswordModel : PageModel
{
    private readonly IAuthService _authService;

    public ChangePasswordModel(IAuthService authService)
    {
        _authService = authService;
    }

    [BindProperty]
    public ChangePasswordViewModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public void OnGet()
    {
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

        var (success, errorMessage) = await _authService.ChangePasswordAsync(
            userGuid,
            Input.CurrentPassword,
            Input.NewPassword);

        if (!success)
        {
            ErrorMessage = errorMessage ?? "Đổi mật khẩu thất bại.";
            return Page();
        }

        SuccessMessage = "Đổi mật khẩu thành công!";
        ModelState.Clear();
        Input = new();
        return Page();
    }
}

using LMS.Models.Entities;
using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace LMS.Pages.Admin.Users;

public class EditModel : PageModel
{
    private readonly IAdminUserService _adminUserService;
    private readonly IUserService _userService;

    public EditModel(IAdminUserService adminUserService, IUserService userService)
    {
        _adminUserService = adminUserService;
        _userService = userService;
    }

    [BindProperty]
    public UserEditViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _adminUserService.GetUserByIdAsync(id, ct);
        if (user == null)
        {
            return NotFound();
        }

        ViewModel = new UserEditViewModel
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            RoleDesc = user.RoleDesc,
            IsActive = user.IsActive,
            Avatar = user.Avatar,
            CoverImageUrl = user.CoverImageUrl,
            AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" }
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }

        var user = await _adminUserService.GetUserByIdAsync(ViewModel.UserId, ct);
        if (user == null)
        {
            return NotFound();
        }

        // Check if username exists (excluding current user)
        if (await _userService.IsUsernameExistsAsync(ViewModel.Username, ViewModel.UserId, ct))
        {
            ModelState.AddModelError(nameof(ViewModel.Username), "Username already exists.");
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }

        // Check if email exists (excluding current user)
        if (await _userService.IsEmailExistsAsync(ViewModel.Email, ViewModel.UserId, ct))
        {
            ModelState.AddModelError(nameof(ViewModel.Email), "Email already exists.");
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }

        // Update user properties
        user.Username = ViewModel.Username;
        user.Email = ViewModel.Email;
        user.FullName = ViewModel.FullName;
        user.Phone = ViewModel.Phone;
        user.RoleDesc = ViewModel.RoleDesc;
        user.IsActive = ViewModel.IsActive;
        user.Avatar = ViewModel.Avatar;
        user.CoverImageUrl = ViewModel.CoverImageUrl;

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(ViewModel.Password))
        {
            user.PasswordHash = HashPassword(ViewModel.Password);
        }

        try
        {
            await _adminUserService.UpdateUserAsync(user, ct);
            TempData["SuccessMessage"] = "User updated successfully.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error updating user: {ex.Message}");
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}


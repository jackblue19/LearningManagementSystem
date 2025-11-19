using LMS.Models.Entities;
using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;

namespace LMS.Pages.Admin.Users;

public class CreateModel : PageModel
{
    private readonly IAdminUserService _adminUserService;
    private readonly IUserService _userService;

    public CreateModel(IAdminUserService adminUserService, IUserService userService)
    {
        _adminUserService = adminUserService;
        _userService = userService;
    }

    [BindProperty]
    public UserCreateViewModel ViewModel { get; set; } = new();

    public void OnGet()
    {
        ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }

        // Check if username exists
        if (await _userService.IsUsernameExistsAsync(ViewModel.Username, null, ct))
        {
            ModelState.AddModelError(nameof(ViewModel.Username), "Username already exists.");
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }

        // Check if email exists
        if (await _userService.IsEmailExistsAsync(ViewModel.Email, null, ct))
        {
            ModelState.AddModelError(nameof(ViewModel.Email), "Email already exists.");
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }

        // Hash password
        var passwordHash = HashPassword(ViewModel.Password);

        var user = new User
        {
            Username = ViewModel.Username,
            Email = ViewModel.Email,
            PasswordHash = passwordHash,
            FullName = ViewModel.FullName,
            Phone = ViewModel.Phone,
            RoleDesc = ViewModel.RoleDesc,
            IsActive = ViewModel.IsActive,
            Avatar = ViewModel.Avatar,
            CoverImageUrl = ViewModel.CoverImageUrl
        };

        try
        {
            await _adminUserService.CreateUserAsync(user, ct);
            TempData["SuccessMessage"] = "User created successfully.";
            return RedirectToPage("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error creating user: {ex.Message}");
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


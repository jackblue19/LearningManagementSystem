using LMS.Models.Entities;
using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using LMS.Services.Interfaces.CommonService; // Đảm bảo có namespace này
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
// Xóa các using System.Security.Cryptography nếu không dùng nữa

namespace LMS.Pages.Admin.Users;

public class CreateModel : PageModel
{
    private readonly IAdminUserService _adminUserService;
    private readonly IUserService _userService;
    private readonly IAuthService _authService; // 1. Inject thêm AuthService

    // 2. Thêm IAuthService vào Constructor
    public CreateModel(
        IAdminUserService adminUserService,
        IUserService userService,
        IAuthService authService)
    {
        _adminUserService = adminUserService;
        _userService = userService;
        _authService = authService;
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

        // Check username/email (Giữ nguyên code cũ của bạn)
        if (await _userService.IsUsernameExistsAsync(ViewModel.Username, null, ct))
        {
            ModelState.AddModelError(nameof(ViewModel.Username), "Username already exists.");
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }

        if (await _userService.IsEmailExistsAsync(ViewModel.Email, null, ct))
        {
            ModelState.AddModelError(nameof(ViewModel.Email), "Email already exists.");
            ViewModel.AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" };
            return Page();
        }

        // 3. SỬA ĐOẠN NÀY: Sử dụng Hash của AuthService để có Salt "LMS_SALT_2025"
        // Lưu ý: Nếu _adminUserService.CreateUserAsync bên trong nó CŨNG Hash, 
        // thì bạn truyền ViewModel.Password (pass thô) vào, hoặc sửa Service đó bỏ Hash đi.
        // Giả định Service chỉ lưu DB, ta Hash ở đây cho chuẩn logic Login:
        var passwordHash = _authService.HashPassword(ViewModel.Password);

        var user = new User
        {
            Username = ViewModel.Username,
            Email = ViewModel.Email,
            PasswordHash = passwordHash, // Đã hash chuẩn Salt
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

    // 4. XÓA hàm HashPassword private static ở dưới cùng đi để tránh nhầm lẫn
}

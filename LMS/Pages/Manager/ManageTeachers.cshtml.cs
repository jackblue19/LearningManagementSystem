using LMS.Models.Entities;
using LMS.Services.Interfaces.ManagerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class ManageTeachersModel : PageModel
{
    private readonly ITeacherManagementService _teacherService;

    public ManageTeachersModel(ITeacherManagementService teacherService)
    {
        _teacherService = teacherService;
    }

    public List<User> Teachers { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int pageNumber = 1)
    {
        PageNumber = pageNumber > 0 ? pageNumber : 1;

        // Get success message from TempData
        if (TempData.ContainsKey("SuccessMessage"))
        {
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
        }

        var (teachers, totalCount) = await _teacherService.GetTeachersAsync(
            searchTerm: SearchTerm,
            isActive: IsActive,
            pageNumber: PageNumber,
            pageSize: PageSize);

        Teachers = teachers;
        TotalCount = totalCount;
    }

    public async Task<IActionResult> OnPostDeactivateAsync(Guid teacherId)
    {
        var (success, errorMessage) = await _teacherService.DeleteTeacherAsync(teacherId);

        if (success)
        {
            SuccessMessage = "Đã vô hiệu hóa tài khoản giáo viên thành công.";
        }
        else
        {
            ErrorMessage = errorMessage;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostActivateAsync(Guid teacherId)
    {
        var (success, errorMessage) = await _teacherService.ActivateTeacherAsync(teacherId);

        if (success)
        {
            SuccessMessage = "Đã kích hoạt tài khoản giáo viên thành công.";
        }
        else
        {
            ErrorMessage = errorMessage;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync(Guid teacherId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.";
            return RedirectToPage();
        }

        var (success, errorMessage) = await _teacherService.ResetTeacherPasswordAsync(teacherId, newPassword);

        if (success)
        {
            SuccessMessage = "Đã đặt lại mật khẩu thành công.";
        }
        else
        {
            ErrorMessage = errorMessage;
        }

        return RedirectToPage();
    }
}

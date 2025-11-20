using LMS.Models.Entities;
using LMS.Models.ViewModels.Manager;
using LMS.Services.Interfaces.ManagerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class EditTeacherModel : PageModel
{
    private readonly ITeacherManagementService _teacherService;

    public EditTeacherModel(ITeacherManagementService teacherService)
    {
        _teacherService = teacherService;
    }

    [BindProperty]
    public EditTeacherViewModel Input { get; set; } = new();

    public User? Teacher { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Teacher = await _teacherService.GetTeacherByIdAsync(id);

        if (Teacher == null)
        {
            return RedirectToPage("/Manager/ManageTeachers");
        }

        Input = new EditTeacherViewModel
        {
            UserId = Teacher.UserId,
            Email = Teacher.Email,
            FullName = Teacher.FullName ?? string.Empty,
            Phone = Teacher.Phone,
            IsActive = Teacher.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Teacher = await _teacherService.GetTeacherByIdAsync(Input.UserId);
            return Page();
        }

        var (success, teacher, errorMessage) = await _teacherService.UpdateTeacherAsync(Input);

        if (!success)
        {
            ErrorMessage = errorMessage;
            Teacher = await _teacherService.GetTeacherByIdAsync(Input.UserId);
            return Page();
        }

        TempData["SuccessMessage"] = "Đã cập nhật thông tin giáo viên thành công.";
        return RedirectToPage("/Manager/ManageTeachers");
    }
}

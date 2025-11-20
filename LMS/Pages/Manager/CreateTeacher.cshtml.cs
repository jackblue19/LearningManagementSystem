using LMS.Models.ViewModels.Manager;
using LMS.Services.Interfaces.ManagerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class CreateTeacherModel : PageModel
{
    private readonly ITeacherManagementService _teacherService;

    public CreateTeacherModel(ITeacherManagementService teacherService)
    {
        _teacherService = teacherService;
    }

    [BindProperty]
    public CreateTeacherViewModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (success, teacher, errorMessage) = await _teacherService.CreateTeacherAsync(Input);

        if (!success)
        {
            ErrorMessage = errorMessage;
            return Page();
        }

        TempData["SuccessMessage"] = $"Đã tạo tài khoản giáo viên {teacher!.Username} thành công.";
        return RedirectToPage("/Manager/ManageTeachers");
    }
}

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Models.ViewModels.StudentService;
using LMS.Services.Interfaces.StudentService;
using LMS.Models.ViewModels;

namespace LMS.Pages.Student.temp;

public class MyCoursesModel : PageModel
{
    private readonly IStudentCourseService _courseSvc;
    private readonly IClassRegistrationService _regSvc;

    public MyCoursesModel(IStudentCourseService courseSvc, IClassRegistrationService regSvc)
    {
        _courseSvc = courseSvc;
        _regSvc = regSvc;
    }

    [BindProperty(SupportsGet = true)]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public PagedResult<StudentCourseListItemVm>? Page { get; set; }
    public string? Flash { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Page = await _courseSvc.ListMyClassesAsync(StudentId, PageIndex, PageSize, ct);
        Flash = TempData["flash"] as string;
    }

    public async Task<IActionResult> OnPostCancelAsync(Guid classId, CancellationToken ct)
    {
        var ok = await _regSvc.CancelAsync(StudentId, classId, ct);
        TempData["flash"] = ok ? "Đã hủy đăng ký lớp." : "Không thể hủy đăng ký.";
        return RedirectToPage(new { studentId = StudentId, pageIndex = PageIndex, pageSize = PageSize });
    }
}

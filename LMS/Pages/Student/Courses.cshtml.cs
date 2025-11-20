using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Models.ViewModels.StudentService;
using LMS.Services.Interfaces.StudentService;
using LMS.Models.ViewModels;

namespace LMS.Pages.Student;

public class CoursesModel : PageModel
{
    private readonly IStudentCourseService _courseSvc;
    private readonly IClassRegistrationService _regSvc;

    public CoursesModel(IStudentCourseService courseSvc, IClassRegistrationService regSvc)
    {
        _courseSvc = courseSvc;
        _regSvc = regSvc;
    }

    [BindProperty(SupportsGet = true)]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? CenterId { get; set; }

    [BindProperty(SupportsGet = true)]
    public long? SubjectId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public PagedResult<StudentCourseListItemVm>? Page { get; set; }
    public string? Flash { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Page = await _courseSvc.ListAvailableClassesAsync(
            StudentId, Search, CenterId, SubjectId, PageIndex, PageSize, ct);
        Flash = TempData["flash"] as string;
    }

    public async Task<IActionResult> OnPostRegisterAsync(Guid classId, CancellationToken ct)
    {
        var ok = await _regSvc.RegisterAsync(StudentId, classId, ct);
        TempData["flash"] = ok
            ? "Đăng ký lớp thành công."
            : "Bạn đã đăng ký lớp này hoặc lớp không khả dụng.";
        return RedirectToPage(new
        {
            studentId = StudentId,
            search = Search,
            centerId = CenterId,
            subjectId = SubjectId,
            pageIndex = PageIndex,
            pageSize = PageSize
        });
    }
}

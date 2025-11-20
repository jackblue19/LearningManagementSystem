using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Models.ViewModels.StudentService;
using LMS.Services.Interfaces.StudentService;
using LMS.Models.ViewModels;

namespace LMS.Pages.Student;

public class ExamsModel : PageModel
{
    private readonly IStudentExamService _examSvc;

    public ExamsModel(IStudentExamService examSvc) => _examSvc = examSvc;

    [BindProperty(SupportsGet = true)]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? To { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool UpcomingOnly { get; set; } = false;

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public PagedResult<StudentExamVm>? Page { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Page = await _examSvc.ListExamsAsync(StudentId, From, To, UpcomingOnly, PageIndex, PageSize, ct);
    }
}

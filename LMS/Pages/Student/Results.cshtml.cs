using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Models.ViewModels.StudentService;
using LMS.Services.Interfaces.StudentService;
using LMS.Models.ViewModels;

namespace LMS.Pages.Student;

public class ResultsModel : PageModel
{
    private readonly IStudentExamResultService _resultSvc;

    public ResultsModel(IStudentExamResultService resultSvc) => _resultSvc = resultSvc;

    [BindProperty(SupportsGet = true)]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public PagedResult<StudentExamResultVm>? Page { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
        => Page = await _resultSvc.ListMyResultsAsync(StudentId, PageIndex, PageSize, ct);
}

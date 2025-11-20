using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Models.ViewModels.StudentService;
using LMS.Services.Interfaces.StudentService;
using LMS.Models.ViewModels;

namespace LMS.Pages.Student;

public class RegistrationsModel : PageModel
{
    private readonly IClassRegistrationService _regSvc;

    public RegistrationsModel(IClassRegistrationService regSvc)
    {
        _regSvc = regSvc;
    }

    [BindProperty(SupportsGet = true)]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public PagedResult<StudentRegisteredClassVm>? Page { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
        => Page = await _regSvc.ListMyRegistrationsAsync(StudentId, PageIndex, PageSize, ct);
}

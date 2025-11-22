using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Services.Centers;
using LMS.Models.ViewModels;
using LMS.Models.Entities;

namespace LMS.Pages.Centers;

public sealed class DetailsModel : PageModel
{
    private readonly ICenterBrowseService _svc;

    public DetailsModel(ICenterBrowseService svc) => _svc = svc;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)] public long? SubjectId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? TeacherId { get; set; }
    [BindProperty(SupportsGet = true)] public int Page { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int Size { get; set; } = 9;

    public Center? Center { get; private set; }
    public IReadOnlyList<Subject> Subjects { get; private set; } = Array.Empty<Subject>();
    public IReadOnlyList<User> Teachers { get; private set; } = Array.Empty<User>();
    public PagedResult<Class> Classes { get; private set; } = new(Array.Empty<Class>(), 0, 1, 9);

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Center = await _svc.GetCenterAsync(Id, ct);
        if (Center is null) return NotFound();

        Subjects = await _svc.ListSubjectsByCenterAsync(Id, ct);
        Teachers = await _svc.ListTeachersAsync(ct);

        Classes = await _svc.PagedClassesAsync(
            centerId: Id,
            subjectId: SubjectId,
            teacherId: TeacherId,
            pageIndex: Page,
            pageSize: Size,
            ct: ct);

        return Page();
    }
}

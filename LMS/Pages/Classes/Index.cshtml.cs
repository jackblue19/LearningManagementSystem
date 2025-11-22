using LMS.Models.Entities;
using LMS.Services.Interfaces.CommonService;
using LMS.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Classes;

public class IndexModel : PageModel
{
    private readonly ICrudService<Class, Guid> _classService;
    private readonly IAuthService _auth;

    public IReadOnlyList<Class> Classes { get; set; } = [];

    public IndexModel(ICrudService<Class, Guid> classService, IAuthService authService)
    {
        _classService = classService;
        _auth = authService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var teacherId = _auth.GetUserId();

        Classes = (await _classService.ListAsync(
            predicate: c => c.TeacherId == teacherId
        )).Items;

        return Page();
    }
}

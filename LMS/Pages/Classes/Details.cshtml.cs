using LMS.Models.Entities;
using LMS.Services.Interfaces.CommonService;
using LMS.Services.Interfaces;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Classes;

public class DetailsModel : PageModel
{
    private readonly ICrudService<Class, Guid> _classSvc;
    private readonly ICrudService<ClassRegistration, long> _regSvc;
    private readonly IAuthService _auth;

    public Class Class { get; set; } = null!;

    public DetailsModel(
        ICrudService<Class, Guid> classSvc,
        ICrudService<ClassRegistration, long> regSvc,
        IAuthService auth)
    {
        _classSvc = classSvc;
        _regSvc = regSvc;
        _auth = auth;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Class = await _classSvc.GetByIdAsync(id,
            includes: new Expression<Func<Class, object>>[]
            {
                c => c.Subject,
                c => c.Teacher
            });

        if (Class is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostRegisterAsync(Guid id)
    {
        var studentId = _auth.GetUserId();
        if (studentId == Guid.Empty)
            return Unauthorized();

        var exists = await _regSvc.ExistsAsync(r => r.StudentId == studentId && r.ClassId == id);
        if (exists)
            return RedirectToPage("/Student/temp/Checkout", new { classId = id });

        var reg = new ClassRegistration
        {
            ClassId = id,
            StudentId = studentId,
            RegistrationStatus = "pending",
            RegisteredAt = DateTime.UtcNow
        };

        await _regSvc.CreateAsync(reg, saveNow: true);
        return RedirectToPage("/Student/temp/Checkout", new { classId = id });
    }
}


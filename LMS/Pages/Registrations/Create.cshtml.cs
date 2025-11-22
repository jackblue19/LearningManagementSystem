using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Pages._Base;
using LMS.Services.Student;
using LMS.Repositories.Abstraction;
using LMS.Models.Entities;
using LMS.Repositories;

namespace LMS.Pages.Registrations;

[Authorize(Roles = "student")]
public sealed class CreateModel : AppPageModel
{
    private readonly IRegistrationService _regSvc;
    private readonly IGenericRepository<Class, Guid> _classes;

    public CreateModel(IRegistrationService regSvc, IGenericRepository<Class, Guid> classes)
    { _regSvc = regSvc; _classes = classes; }

    [BindProperty(SupportsGet = true)] public Guid ClassId { get; set; }
    public Class? Class { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Class = await _classes.GetByIdAsync(ClassId, asNoTracking: true, ct: ct);
        return Class is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var cls = await _classes.GetByIdAsync(ClassId, asNoTracking: true, ct: ct);
        if (cls is null) { TempData["Error"] = "Lớp không tồn tại."; return RedirectToPage("/Centers/Index"); }

        var studentId = CurrentUserId;
        var ok = await _regSvc.CanRegisterAsync(studentId, ClassId, ct);
        if (!ok)
        {
            TempData["Error"] = "Bạn đã đăng ký lớp này hoặc lớp không hợp lệ.";
            //return RedirectToPage("/Centers/Details", new { id = cls.CenterId });
            return RedirectToPage("/Student/Schedule/Index");
        }

        await _regSvc.RegisterAsync(studentId, ClassId, null, ct);
        TempData["Success"] = "Đăng ký thành công.";
        //return RedirectToPage("/Centers/Details", new { id = cls.CenterId });
        return RedirectToPage("/Student/Schedule/Index");
    }
}

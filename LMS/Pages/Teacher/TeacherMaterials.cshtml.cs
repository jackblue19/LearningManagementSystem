using System.Security.Claims;
using LMS.Models.Entities;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Teacher;

[Authorize(Policy = "TeacherOnly")]
public class TeacherMaterialsModel : PageModel
{
    private readonly IMaterialService _materialService;
    private readonly IClassManagementService _classService;

    public TeacherMaterialsModel(
        IMaterialService materialService,
        IClassManagementService classService)
    {
        _materialService = materialService;
        _classService = classService;
    }

    public IReadOnlyList<ClassMaterial> Materials { get; set; } = new List<ClassMaterial>();
    public IReadOnlyList<Class> TeacherClasses { get; set; } = new List<Class>();

    [BindProperty]
    public Guid SelectedClassId { get; set; }

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string? MaterialType { get; set; }

    [BindProperty]
    public string? Note { get; set; }

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? classId, CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        await LoadTeacherContextAsync(teacherId, classId, ct);
        if (!TeacherClasses.Any())
        {
            ErrorMessage ??= "You are not assigned to any class yet.";
        }
        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Invalid input. Please check your form.";

            if (TryGetTeacherId(out var teacherId))
            {
                await LoadTeacherContextAsync(teacherId, SelectedClassId, ct);
            }

            return Page();
        }

        if (UploadFile == null || UploadFile.Length == 0)
        {
            ErrorMessage = "Please select a file to upload.";

            if (TryGetTeacherId(out var teacherId))
            {
                await LoadTeacherContextAsync(teacherId, SelectedClassId, ct);
            }

            return Page();
        }

        try
        {
            if (!TryGetTeacherId(out var teacherId))
            {
                return Challenge();
            }

            await _materialService.UploadMaterialAsync(
                classId: SelectedClassId,
                uploadedByUserId: teacherId,
                title: Title,
                file: UploadFile,
                materialType: MaterialType,
                note: Note,
                ct: ct);

            Message = "Material uploaded successfully!";
            return RedirectToPage(new { classId = SelectedClassId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error uploading material: {ex.Message}";
            if (TryGetTeacherId(out var teacherId))
            {
                await LoadTeacherContextAsync(teacherId, SelectedClassId, ct);
            }
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid materialId, CancellationToken ct = default)
    {
        try
        {
            if (!TryGetTeacherId(out var teacherId))
            {
                return Challenge();
            }

            var deleted = await _materialService.DeleteMaterialAsync(materialId, ct);
            if (deleted)
            {
                Message = "Material deleted successfully!";
            }
            else
            {
                ErrorMessage = "Material not found.";
            }

            return RedirectToPage(new { classId = SelectedClassId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error deleting material: {ex.Message}";
            if (TryGetTeacherId(out var teacherId))
            {
                await LoadTeacherContextAsync(teacherId, SelectedClassId, ct);
            }
            return Page();
        }
    }

    private bool TryGetTeacherId(out Guid teacherId)
    {
        teacherId = Guid.Empty;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out teacherId);
    }

    private async Task LoadTeacherContextAsync(Guid teacherId, Guid? classId, CancellationToken ct)
    {
        TeacherClasses = await _classService.GetClassesByTeacherIdAsync(teacherId, ct);

        if (!TeacherClasses.Any())
        {
            Materials = Array.Empty<ClassMaterial>();
            SelectedClassId = Guid.Empty;
            ErrorMessage ??= "You are not assigned to any class yet.";
            return;
        }

        if (classId.HasValue && TeacherClasses.Any(c => c.ClassId == classId.Value))
        {
            SelectedClassId = classId.Value;
        }
        else if (SelectedClassId != Guid.Empty && TeacherClasses.Any(c => c.ClassId == SelectedClassId))
        {
            // keep current selection if still valid
        }
        else
        {
            SelectedClassId = TeacherClasses.First().ClassId;
        }

        Materials = SelectedClassId == Guid.Empty
            ? Array.Empty<ClassMaterial>()
            : await _materialService.GetMaterialsByClassIdAsync(SelectedClassId, ct);
    }
}

using LMS.Models.Entities;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LMS.Pages.Teacher;

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

    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? classId, CancellationToken ct = default)
    {
        // TODO: Get teacher ID from session/authentication
        var teacherId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Placeholder

        // Get teacher's classes for dropdown
        // TeacherClasses = await _classService.GetClassesByTeacherIdAsync(teacherId, ct);

        if (classId.HasValue)
        {
            SelectedClassId = classId.Value;
            Materials = await _materialService.GetMaterialsByClassIdAsync(classId.Value, ct);
        }
        else
        {
            // Show all materials for teacher
            Materials = await _materialService.GetMaterialsByTeacherIdAsync(teacherId, ct);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Invalid input. Please check your form.";
            return Page();
        }

        if (UploadFile == null || UploadFile.Length == 0)
        {
            ErrorMessage = "Please select a file to upload.";
            return Page();
        }

        try
        {
            // TODO: Get teacher ID from session/authentication
            var teacherId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Placeholder

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
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid materialId, CancellationToken ct = default)
    {
        try
        {
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
            return Page();
        }
    }
}

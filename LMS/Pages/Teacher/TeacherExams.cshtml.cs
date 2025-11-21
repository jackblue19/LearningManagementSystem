using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using LMS.Models.Entities;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Teacher;

[Authorize(Policy = "TeacherOnly")]
public class TeacherExamsModel : PageModel
{
    private readonly IClassManagementService _classService;
    private readonly IExamService _examService;

    public TeacherExamsModel(
        IClassManagementService classService,
        IExamService examService)
    {
        _classService = classService;
        _examService = examService;
    }

    public IReadOnlyList<Class> TeacherClasses { get; private set; } = new List<Class>();
    public Class? SelectedClass { get; private set; }
    public IReadOnlyList<Exam> Exams { get; private set; } = new List<Exam>();
    public bool IsEditing => Input.ExamId.HasValue;

    [BindProperty]
    public ExamInput Input { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? classId, Guid? examId, CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        Exam? examToEdit = null;
        if (examId.HasValue)
        {
            var includes = new Expression<Func<Exam, object>>[] { e => e.Class };
            examToEdit = await _examService.GetByIdAsync(examId.Value, includes: includes, ct: ct);
            if (examToEdit is null || examToEdit.TeacherId != teacherId)
            {
                return NotFound();
            }

            Input = new ExamInput
            {
                ExamId = examToEdit.ExamId,
                ClassId = examToEdit.ClassId,
                Title = examToEdit.Title,
                ExamType = examToEdit.ExamType,
                ExamDate = examToEdit.ExamDate,
                DurationMin = examToEdit.DurationMin,
                MaxScore = examToEdit.MaxScore,
                Description = examToEdit.ExamDesc
            };
        }

        var preferredClassId = classId ?? examToEdit?.ClassId;
        await LoadTeacherDataAsync(teacherId, preferredClassId, ct);

        if (!TeacherClasses.Any())
        {
            ErrorMessage ??= "You are not assigned to any class yet.";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        if (Input.ClassId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(Input.ClassId), "Class selection is required.");
        }

        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "Title is required.");
        }

        if (Input.MaxScore.HasValue && Input.MaxScore.Value < 0)
        {
            ModelState.AddModelError(nameof(Input.MaxScore), "Maximum score must be non-negative.");
        }

        if (!ModelState.IsValid)
        {
            await LoadTeacherDataAsync(teacherId, Input.ClassId, ct);
            ErrorMessage = "Please correct the highlighted errors.";
            return Page();
        }

        try
        {
            if (SelectedClass is null)
            {
                ErrorMessage ??= "You are not assigned to any class yet.";
                return Page();
            }

            if (Input.ExamId.HasValue)
            {
                var existing = await _examService.GetByIdAsync(Input.ExamId.Value, ct: ct);
                if (existing is null || existing.TeacherId != teacherId)
                {
                    return NotFound();
                }

                var updatedExam = new Exam
                {
                    ExamId = Input.ExamId.Value,
                    ClassId = Input.ClassId,
                    Title = Input.Title,
                    ExamType = Input.ExamType,
                    ExamDate = Input.ExamDate,
                    DurationMin = Input.DurationMin,
                    ExamDesc = Input.Description,
                    MaxScore = Input.MaxScore,
                    TeacherId = teacherId
                };

                await _examService.UpdateAsync(updatedExam, saveNow: true, ct);
                Message = "Exam updated successfully.";
            }
            else
            {
                var newExam = new Exam
                {
                    ExamId = Guid.NewGuid(),
                    ClassId = Input.ClassId,
                    Title = Input.Title,
                    ExamType = Input.ExamType,
                    ExamDate = Input.ExamDate,
                    DurationMin = Input.DurationMin,
                    ExamDesc = Input.Description,
                    MaxScore = Input.MaxScore,
                    TeacherId = teacherId
                };

                await _examService.CreateAsync(newExam, saveNow: true, ct);
                Message = "Exam created successfully.";
            }

            return RedirectToPage(new { classId = Input.ClassId });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            await LoadTeacherDataAsync(teacherId, Input.ClassId, ct);
            return Page();
        }
    }

    public async Task<IActionResult> OnPostPublishAsync(Guid examId, CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        var exam = await _examService.GetByIdAsync(examId, ct: ct);
        if (exam is null || exam.TeacherId != teacherId)
        {
            return NotFound();
        }

        try
        {
            var published = await _examService.PublishExamAsync(examId, ct);
            if (published)
            {
                Message = "Exam published successfully.";
            }
            else
            {
                ErrorMessage = "Unable to publish exam.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage(new { classId = exam.ClassId });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid examId, Guid classId, CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        var exam = await _examService.GetByIdAsync(examId, ct: ct);
        if (exam is null || exam.TeacherId != teacherId)
        {
            return NotFound();
        }

        try
        {
            await _examService.DeleteByIdAsync(examId, saveNow: true, ct);
            Message = "Exam deleted.";
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }

        return RedirectToPage(new { classId });
    }

    private bool TryGetTeacherId(out Guid teacherId)
    {
        teacherId = Guid.Empty;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out teacherId);
    }

    private async Task LoadTeacherDataAsync(Guid teacherId, Guid? preferredClassId, CancellationToken ct)
    {
        TeacherClasses = await _classService.GetClassesByTeacherIdAsync(teacherId, ct);

        if (!TeacherClasses.Any())
        {
            SelectedClass = null;
            Exams = Array.Empty<Exam>();
            if (Input == null)
            {
                Input = new ExamInput();
            }
            Input.ClassId = Guid.Empty;
            ErrorMessage ??= "You are not assigned to any class yet.";
            return;
        }

        SelectedClass = preferredClassId.HasValue
            ? TeacherClasses.FirstOrDefault(c => c.ClassId == preferredClassId.Value)
            : null;

        SelectedClass ??= TeacherClasses.First();

        if (Input == null)
        {
            Input = new ExamInput();
        }

        if (!Input.ExamId.HasValue)
        {
            Input.ClassId = SelectedClass.ClassId;
        }

        var exams = await _examService.GetExamsByClassAsync(SelectedClass.ClassId, ct);
        Exams = exams
            .OrderBy(e => e.ExamDate ?? DateOnly.MaxValue)
            .ThenBy(e => e.Title)
            .ToList();
    }

    public sealed class ExamInput
    {
        public Guid? ExamId { get; set; }

        [Required]
        public Guid ClassId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(40)]
        public string? ExamType { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? ExamDate { get; set; }

        [Range(0, 600)]
        public int? DurationMin { get; set; }

        [Range(0, 1000)]
        public decimal? MaxScore { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using LMS.Data;
using LMS.Models.Entities;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Teacher;

[Authorize(Policy = "TeacherOnly")]
public class TeacherExamResultsModel : PageModel
{
    private readonly IExamService _examService;
    private readonly IExamResultService _examResultService;
    private readonly CenterDbContext _db;

    public TeacherExamResultsModel(
        IExamService examService,
        IExamResultService examResultService,
        CenterDbContext db)
    {
        _examService = examService;
        _examResultService = examResultService;
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public Guid ExamId { get; set; }

    public Exam? Exam { get; private set; }
    public IReadOnlyList<ResultInput> ResultInputs { get; private set; } = new List<ResultInput>();
    public decimal AverageScore { get; private set; }
    public int PassCount { get; private set; }

    [BindProperty]
    public List<ResultInput> EditableResults { get; set; } = new();

    [TempData]
    public string? Message { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        var loaded = await LoadExamContextAsync(teacherId, ExamId, ct);
        if (!loaded)
        {
            return NotFound();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!TryGetTeacherId(out var teacherId))
        {
            return Challenge();
        }

        var postedResults = EditableResults
            .Select(r => new ResultInput
            {
                StudentId = r.StudentId,
                StudentName = r.StudentName,
                StudentEmail = r.StudentEmail,
                Score = r.Score,
                Note = r.Note
            })
            .ToList();

        var loaded = await LoadExamContextAsync(teacherId, ExamId, ct);
        if (!loaded)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ApplyPostedValues(postedResults);
            ErrorMessage = "Please correct the highlighted errors.";
            return Page();
        }

        try
        {
            var existingResults = (await _examResultService.GetResultsByExamAsync(ExamId, ct))
                .ToDictionary(r => r.StudentId, r => r);

            foreach (var input in postedResults)
            {
                var trimmedNote = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim();

                if (input.Score.HasValue)
                {
                    if (existingResults.TryGetValue(input.StudentId, out var existing))
                    {
                        if (existing.Score != input.Score || existing.Note != trimmedNote)
                        {
                            existing.Score = input.Score;
                            existing.Note = trimmedNote;
                            await _examResultService.UpdateResultAsync(new ExamResult
                            {
                                ResultId = existing.ResultId,
                                ExamId = ExamId,
                                StudentId = input.StudentId,
                                Score = input.Score,
                                Note = trimmedNote
                            }, ct);
                        }
                    }
                    else
                    {
                        await _examResultService.SubmitResultAsync(new ExamResult
                        {
                            ExamId = ExamId,
                            StudentId = input.StudentId,
                            Score = input.Score,
                            Note = trimmedNote
                        }, ct);
                    }
                }
                else if (existingResults.TryGetValue(input.StudentId, out var existing))
                {
                    await _examResultService.DeleteResultAsync(existing.ResultId, ct);
                }
            }

            Message = "Đã cập nhật điểm thi.";
            return RedirectToPage(new { examId = ExamId });
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return Page();
        }
    }

    private bool TryGetTeacherId(out Guid teacherId)
    {
        teacherId = Guid.Empty;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userId, out teacherId);
    }

    private async Task<bool> LoadExamContextAsync(Guid teacherId, Guid examId, CancellationToken ct)
    {
        var includes = new Expression<Func<Exam, object>>[] { e => e.Class };
        Exam = await _examService.GetByIdAsync(examId, includes: includes, ct: ct);

        if (Exam is null || Exam.TeacherId != teacherId)
        {
            return false;
        }

        var students = await _db.ClassRegistrations
            .AsNoTracking()
            .Include(cr => cr.Student)
            .Where(cr => cr.ClassId == Exam.ClassId &&
                        (cr.RegistrationStatus == "approved" || cr.RegistrationStatus == "active"))
            .OrderBy(cr => cr.Student.FullName)
            .ToListAsync(ct);

        var results = (await _examResultService.GetResultsByExamAsync(examId, ct))
            .ToDictionary(r => r.StudentId, r => r);

        ResultInputs = students.Select(cr =>
        {
            results.TryGetValue(cr.StudentId, out var result);
            return new ResultInput
            {
                StudentId = cr.StudentId,
                StudentName = cr.Student.FullName ?? cr.Student.Username,
                StudentEmail = cr.Student.Email ?? string.Empty,
                Score = result?.Score,
                Note = result?.Note
            };
        }).ToList();

        EditableResults = ResultInputs.Select(r => new ResultInput
        {
            StudentId = r.StudentId,
            StudentName = r.StudentName,
            StudentEmail = r.StudentEmail,
            Score = r.Score,
            Note = r.Note
        }).ToList();

        AverageScore = await _examResultService.GetAverageScoreAsync(examId, ct);
        PassCount = await _examResultService.GetPassCountAsync(examId, passingScore: Exam.MaxScore.HasValue ? Exam.MaxScore.Value * 0.5m : 5m, ct);

        return true;
    }

    private void ApplyPostedValues(IReadOnlyList<ResultInput> posted)
    {
        var lookup = posted.ToDictionary(r => r.StudentId);
        for (var i = 0; i < EditableResults.Count; i++)
        {
            var studentId = EditableResults[i].StudentId;
            if (!lookup.TryGetValue(studentId, out var postedRow))
            {
                continue;
            }

            EditableResults[i].Score = postedRow.Score;
            EditableResults[i].Note = postedRow.Note;
        }
    }

    public sealed class ResultInput
    {
        public Guid StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        [Range(0, 1000)]
        public decimal? Score { get; set; }

        [StringLength(300)]
        public string? Note { get; set; }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Pages._Base;
using LMS.Services.Student;

namespace LMS.Pages.Student.Exams;

[Authorize(Roles = "student")]
public sealed class IndexModel : AppPageModel
{
    private readonly IStudentExamService _examSvc;
    private readonly IStudentScheduleService _scheduleSvc; // tái dùng để lấy dropdown lớp của tôi

    public IndexModel(IStudentExamService examSvc, IStudentScheduleService scheduleSvc)
    { _examSvc = examSvc; _scheduleSvc = scheduleSvc; }

    [BindProperty(SupportsGet = true)] public Guid? ClassId { get; set; }

    public IReadOnlyList<ExamDto> Exams { get; private set; } = Array.Empty<ExamDto>();
    public IReadOnlyList<ExamScoreDto> Scores { get; private set; } = Array.Empty<ExamScoreDto>();
    public IReadOnlyList<MyClassItem> MyClasses { get; private set; } = Array.Empty<MyClassItem>();

    public async Task OnGetAsync(CancellationToken ct)
    {
        var studentId = CurrentUserId;

        // dropdown lớp (Approved) cho student
        MyClasses = await _scheduleSvc.ListMyClassesAsync(studentId, ct);

        // danh sách đề + điểm (nếu có) theo class filter
        Exams = await _examSvc.ListExamsForStudentAsync(studentId, ClassId, ct);
        Scores = await _examSvc.ListScoresAsync(studentId, ClassId, ct);
    }
}

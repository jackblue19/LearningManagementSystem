using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Assessment;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class ExamService : CrudService<Exam, Guid>, IExamService
{
    private readonly IExamRepository _examRepo;
    private readonly IExamResultRepository _examResultRepo;

    public ExamService(
        IExamRepository examRepo,
        IExamResultRepository examResultRepo) : base(examRepo)
    {
        _examRepo = examRepo;
        _examResultRepo = examResultRepo;
    }

    // Domain-specific queries
    public async Task<IEnumerable<Exam>> GetExamsByClassAsync(Guid classId, CancellationToken ct = default)
    {
        return await _examRepo.GetByClassIdAsync(classId, ct);
    }

    public async Task<IEnumerable<Exam>> GetExamsByTeacherAsync(Guid teacherId, CancellationToken ct = default)
    {
        return await _examRepo.GetByTeacherIdAsync(teacherId, ct);
    }

    public async Task<IEnumerable<Exam>> GetUpcomingExamsAsync(Guid classId, CancellationToken ct = default)
    {
        return await _examRepo.GetUpcomingExamsAsync(classId, ct);
    }

    public async Task<bool> CanScheduleExamAsync(Guid classId, DateOnly examDate, CancellationToken ct = default)
    {
        return !await _examRepo.HasExamOnDateAsync(classId, examDate, ct);
    }

    // ✅ Override base CreateAsync with business logic
    public override async Task<Exam> CreateAsync(Exam exam, bool saveNow = true, CancellationToken ct = default)
    {
        // Validate exam date is not in the past
        if (exam.ExamDate.HasValue && exam.ExamDate.Value < DateOnly.FromDateTime(DateTime.Today))
            throw new InvalidOperationException("Cannot schedule exam in the past.");

        // Check for scheduling conflicts
        if (exam.ExamDate.HasValue)
        {
            var hasConflict = await _examRepo.HasExamOnDateAsync(exam.ClassId, exam.ExamDate.Value, ct);
            if (hasConflict)
                throw new InvalidOperationException("Another exam is already scheduled on this date for this class.");
        }

        exam.ExamStatus = "Draft";
        return await base.CreateAsync(exam, saveNow, ct);
    }

    // ✅ Override base UpdateAsync with business logic
    public override async Task UpdateAsync(Exam exam, bool saveNow = true, CancellationToken ct = default)
    {
        var existing = await _examRepo.GetByIdAsync(exam.ExamId, asNoTracking: false, ct);
        if (existing == null)
            throw new InvalidOperationException("Exam not found.");

        // Prevent changes if exam is already completed
        if (existing.ExamStatus == "Completed")
            throw new InvalidOperationException("Cannot update completed exam.");

        // Check if exam has results - restrict certain updates
        var hasResults = await _examResultRepo.HasResultsAsync(exam.ExamId, ct);
        if (hasResults && existing.MaxScore != exam.MaxScore)
            throw new InvalidOperationException("Cannot change max score after results have been submitted.");

        // Update properties
        existing.Title = exam.Title;
        existing.ExamType = exam.ExamType;
        existing.ExamDate = exam.ExamDate;
        existing.DurationMin = exam.DurationMin;
        existing.ExamDesc = exam.ExamDesc;
        existing.MaxScore = exam.MaxScore;

        await base.UpdateAsync(existing, saveNow, ct);
    }

    // ✅ Override base DeleteByIdAsync with business logic
    public override async Task DeleteByIdAsync(Guid examId, bool saveNow = true, CancellationToken ct = default)
    {
        // Prevent deletion if exam has results
        var hasResults = await _examResultRepo.HasResultsAsync(examId, ct);
        if (hasResults)
            throw new InvalidOperationException("Cannot delete exam with existing results.");

        await base.DeleteByIdAsync(examId, saveNow, ct);
    }

    // Business-specific method
    public async Task<bool> PublishExamAsync(Guid examId, CancellationToken ct = default)
    {
        var exam = await _examRepo.GetByIdAsync(examId, asNoTracking: false, ct);
        if (exam == null)
            return false;

        // Validate required fields
        if (!exam.ExamDate.HasValue)
            throw new InvalidOperationException("Cannot publish exam without a date.");
        if (!exam.MaxScore.HasValue || exam.MaxScore.Value <= 0)
            throw new InvalidOperationException("Cannot publish exam without a valid max score.");

        exam.ExamStatus = "Published";
        await _examRepo.UpdateAsync(exam, saveNow: true, ct);
        return true;
    }
}
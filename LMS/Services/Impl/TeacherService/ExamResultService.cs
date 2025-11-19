using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Assessment;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services.Impl.TeacherService;

public class ExamResultService : CrudService<ExamResult, long>, IExamResultService
{
    private readonly IExamResultRepository _examResultRepo;
    private readonly IExamRepository _examRepo;

    public ExamResultService(
        IExamResultRepository examResultRepo,
        IExamRepository examRepo) : base(examResultRepo)
    {
        _examResultRepo = examResultRepo;
        _examRepo = examRepo;
    }

    public async Task<IEnumerable<ExamResult>> GetResultsByExamAsync(Guid examId, CancellationToken ct = default)
    {
        return await _examResultRepo.GetByExamIdAsync(examId);
    }

    public async Task<ExamResult?> GetStudentExamResultAsync(Guid examId, Guid studentId, CancellationToken ct = default)
    {
        return await _examResultRepo.GetByExamAndStudentAsync(examId, studentId);
    }

    public async Task<ExamResult> SubmitResultAsync(ExamResult result, CancellationToken ct = default)
    {
        // Validate exam exists
        var exam = await _examRepo.GetByIdAsync(result.ExamId, ct: ct);
        if (exam == null)
            throw new InvalidOperationException("Exam not found.");

        // Validate student is enrolled in the exam's class
        var isEnrolled = await _examResultRepo.IsStudentInExamClassAsync(result.ExamId, result.StudentId);
        if (!isEnrolled)
            throw new InvalidOperationException("Student is not enrolled in this exam's class.");

        // Check for duplicate submission
        var existingResult = await _examResultRepo.GetByExamAndStudentAsync(result.ExamId, result.StudentId);
        if (existingResult != null)
            throw new InvalidOperationException("Result already exists for this student and exam.");

        // Validate score
        if (result.Score.HasValue && exam.MaxScore.HasValue && result.Score.Value > exam.MaxScore.Value)
            throw new InvalidOperationException($"Score cannot exceed maximum score of {exam.MaxScore.Value}.");

        if (result.Score.HasValue && result.Score.Value < 0)
            throw new InvalidOperationException("Score cannot be negative.");

        return await _examResultRepo.AddAsync(result, saveNow: true, ct);
    }

    public async Task<ExamResult> UpdateResultAsync(ExamResult result, CancellationToken ct = default)
    {
        var existing = await _examResultRepo.GetByIdAsync(result.ResultId, asNoTracking: false, ct);
        if (existing == null)
            throw new InvalidOperationException("Result not found.");

        var exam = await _examRepo.GetByIdAsync(result.ExamId, ct: ct);
        if (exam == null)
            throw new InvalidOperationException("Exam not found.");

        // Validate score
        if (result.Score.HasValue && exam.MaxScore.HasValue && result.Score.Value > exam.MaxScore.Value)
            throw new InvalidOperationException($"Score cannot exceed maximum score of {exam.MaxScore.Value}.");

        if (result.Score.HasValue && result.Score.Value < 0)
            throw new InvalidOperationException("Score cannot be negative.");

        existing.Score = result.Score;
        existing.Note = result.Note;

        await _examResultRepo.UpdateAsync(existing, saveNow: true, ct);
        return existing;
    }

    public async Task<bool> DeleteResultAsync(long resultId, CancellationToken ct = default)
    {
        var result = await _examResultRepo.GetByIdAsync(resultId, asNoTracking: false, ct);
        if (result == null)
            return false;

        await _examResultRepo.DeleteAsync(result, saveNow: true, ct);
        return true;
    }

    public async Task<bool> CanSubmitResultAsync(Guid examId, Guid studentId, CancellationToken ct = default)
    {
        // Check if student is enrolled in the exam's class
        var isEnrolled = await _examResultRepo.IsStudentInExamClassAsync(examId, studentId);
        if (!isEnrolled)
            return false;

        // Check if result already exists
        var existingResult = await _examResultRepo.GetByExamAndStudentAsync(examId, studentId);
        return existingResult == null;
    }

    public async Task<bool> IsStudentInExamClassAsync(Guid examId, Guid studentId, CancellationToken ct = default)
    {
        return await _examResultRepo.IsStudentInExamClassAsync(examId, studentId);
    }

    public async Task<decimal> GetAverageScoreAsync(Guid examId, CancellationToken ct = default)
    {
        var results = await _examResultRepo.GetByExamIdAsync(examId);
        var scoresWithValues = results.Where(r => r.Score.HasValue).ToList();

        if (!scoresWithValues.Any())
            return 0;

        return scoresWithValues.Average(r => r.Score!.Value);
    }

    public async Task<int> GetPassCountAsync(Guid examId, decimal passingScore, CancellationToken ct = default)
    {
        var results = await _examResultRepo.GetByExamIdAsync(examId);
        return results.Count(r => r.Score.HasValue && r.Score.Value >= passingScore);
    }
}

using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Assessment;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Assessment;

public class ExamResultRepository : GenericRepository<ExamResult, long>, IExamResultRepository
{
    public ExamResultRepository(CenterDbContext db) : base(db) { }

    // Basic queries
    public async Task<bool> HasResultsAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .AnyAsync(er => er.ExamId == examId, ct);
    }

    // Get results for a specific exam

    public async Task<IEnumerable<ExamResult>> GetByExamIdAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Include(er => er.Student)
                        .Include(er => er.Exam)
                        .Where(er => er.ExamId == examId)
                        .OrderByDescending(er => er.Score)
                        .ToListAsync(ct);
    }

    public async Task<ExamResult?> GetByExamAndStudentAsync(Guid examId, Guid studentId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Include(er => er.Exam)
                        .Include(er => er.Student)
                        .FirstOrDefaultAsync(er => er.ExamId == examId && er.StudentId == studentId, ct);
    }



    // Student specific
    public async Task<bool> IsStudentInExamClassAsync(Guid examId, Guid studentId, CancellationToken ct = default)
    {
        var exam = await _db.Set<Exam>()
                            .FirstOrDefaultAsync(e => e.ExamId == examId, ct);
        if (exam == null)
            return false;

        return await _db.Set<ClassRegistration>()
                        .AnyAsync(cr => cr.StudentId == studentId && cr.ClassId == exam.ClassId, ct);
    }
    public async Task<IEnumerable<ExamResult>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Include(er => er.Exam)
                            .ThenInclude(e => e.Class)
                        .Where(er => er.StudentId == studentId)
                        .OrderByDescending(er => er.Exam.ExamDate)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<ExamResult>> GetStudentResultsByClassAsync(Guid studentId, Guid classId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Include(er => er.Exam)
                        .Where(er => er.StudentId == studentId && er.Exam.ClassId == classId)
                        .OrderBy(er => er.Exam.ExamDate)
                        .ToListAsync(ct);
    }

    // Statistics and aggregations
    public async Task<decimal?> GetAverageScoreAsync(Guid examId, CancellationToken ct = default)
    {
        var scores = await _db.Set<ExamResult>()
                              .Where(er => er.ExamId == examId && er.Score.HasValue)
                              .Select(er => er.Score!.Value)
                              .ToListAsync(ct);

        return scores.Any() ? scores.Average() : null;
    }

    public async Task<decimal?> GetHighestScoreAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Where(er => er.ExamId == examId && er.Score.HasValue)
                        .MaxAsync(er => (decimal?)er.Score, ct);
    }

    public async Task<decimal?> GetLowestScoreAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Where(er => er.ExamId == examId && er.Score.HasValue)
                        .MinAsync(er => (decimal?)er.Score, ct);
    }

    public async Task<int> CountPassingStudentsAsync(Guid examId, decimal passingScore, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .CountAsync(er => er.ExamId == examId &&
                                         er.Score.HasValue &&
                                         er.Score.Value >= passingScore, ct);
    }

    public async Task<int> CountFailingStudentsAsync(Guid examId, decimal passingScore, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .CountAsync(er => er.ExamId == examId &&
                                         er.Score.HasValue &&
                                         er.Score.Value < passingScore, ct);
    }

    public async Task<int> CountGradedResultsAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .CountAsync(er => er.ExamId == examId && er.Score.HasValue, ct);
    }

    public async Task<int> CountPendingResultsAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .CountAsync(er => er.ExamId == examId && !er.Score.HasValue, ct);
    }

    // Filtering and sorting
    public async Task<IEnumerable<ExamResult>> GetTopScoresAsync(Guid examId, int topN, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Include(er => er.Student)
                        .Where(er => er.ExamId == examId && er.Score.HasValue)
                        .OrderByDescending(er => er.Score)
                        .Take(topN)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<ExamResult>> GetFailingResultsAsync(Guid examId, decimal passingScore, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Include(er => er.Student)
                        .Where(er => er.ExamId == examId &&
                                    er.Score.HasValue &&
                                    er.Score.Value < passingScore)
                        .OrderBy(er => er.Score)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<ExamResult>> GetPendingGradesAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .Include(er => er.Student)
                        .Where(er => er.ExamId == examId && !er.Score.HasValue)
                        .ToListAsync(ct);
    }

    // Bulk operations
    public async Task<bool> HasPendingResultsAsync(Guid examId, CancellationToken ct = default)
    {
        return await _db.Set<ExamResult>()
                        .AnyAsync(er => er.ExamId == examId && !er.Score.HasValue, ct);
    }

    public async Task<IEnumerable<Guid>> GetStudentsWithoutResultsAsync(Guid examId, CancellationToken ct = default)
    {
        // Get the class ID for this exam
        var exam = await _db.Set<Exam>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(e => e.ExamId == examId, ct);

        if (exam == null)
            return Enumerable.Empty<Guid>();

        // Get all enrolled students
        var enrolledStudents = await _db.Set<ClassRegistration>()
                                        .Where(cr => cr.ClassId == exam.ClassId)
                                        .Select(cr => cr.StudentId)
                                        .ToListAsync(ct);

        // Get students who already have results
        var studentsWithResults = await _db.Set<ExamResult>()
                                           .Where(er => er.ExamId == examId)
                                           .Select(er => er.StudentId)
                                           .ToListAsync(ct);

        // Return the difference
        return enrolledStudents.Except(studentsWithResults);
    }

    // Validation
    public async Task<bool> CanDeleteResultAsync(long resultId, CancellationToken ct = default)
    {
        var result = await _db.Set<ExamResult>()
                              .Include(er => er.Exam)
                              .FirstOrDefaultAsync(er => er.ResultId == resultId, ct);

        if (result == null)
            return false;

        // Can delete if exam is not completed
        return result.Exam.ExamStatus != "Completed";
    }
}

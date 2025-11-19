using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Assessment;

public interface IExamResultRepository : IGenericRepository<ExamResult, long>
{
    // Basic queries
    Task<bool> HasResultsAsync(Guid examId, CancellationToken ct = default);

    // Get results for a specific exam
    Task<IEnumerable<ExamResult>> GetByExamIdAsync(Guid examId, CancellationToken ct = default);
    Task<ExamResult?> GetByExamAndStudentAsync(Guid examId, Guid studentId, CancellationToken ct = default);

    // Student specific
    Task<bool> IsStudentInExamClassAsync(Guid examId, Guid studentId, CancellationToken ct = default);
    Task<IEnumerable<ExamResult>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default);
    Task<IEnumerable<ExamResult>> GetStudentResultsByClassAsync(Guid studentId, Guid classId, CancellationToken ct = default);

    // Statistics and aggregations
    Task<decimal?> GetAverageScoreAsync(Guid examId, CancellationToken ct = default);
    Task<decimal?> GetHighestScoreAsync(Guid examId, CancellationToken ct = default);
    Task<decimal?> GetLowestScoreAsync(Guid examId, CancellationToken ct = default);
    Task<int> CountPassingStudentsAsync(Guid examId, decimal passingScore, CancellationToken ct = default);
    Task<int> CountFailingStudentsAsync(Guid examId, decimal passingScore, CancellationToken ct = default);
    Task<int> CountGradedResultsAsync(Guid examId, CancellationToken ct = default);
    Task<int> CountPendingResultsAsync(Guid examId, CancellationToken ct = default);

    // Filtering and sorting
    Task<IEnumerable<ExamResult>> GetTopScoresAsync(Guid examId, int topN, CancellationToken ct = default);
    Task<IEnumerable<ExamResult>> GetFailingResultsAsync(Guid examId, decimal passingScore, CancellationToken ct = default);
    Task<IEnumerable<ExamResult>> GetPendingGradesAsync(Guid examId, CancellationToken ct = default);

    // Bulk operations
    Task<bool> HasPendingResultsAsync(Guid examId, CancellationToken ct = default);
    Task<IEnumerable<Guid>> GetStudentsWithoutResultsAsync(Guid examId, CancellationToken ct = default);

    // Validation
    Task<bool> CanDeleteResultAsync(long resultId, CancellationToken ct = default);
}

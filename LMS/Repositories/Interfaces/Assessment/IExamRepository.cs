using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Assessment;

public interface IExamRepository : IGenericRepository<Exam, Guid>
{
    // Basic queries
    Task<IEnumerable<Exam>> GetByClassIdAsync(Guid classId, CancellationToken ct = default);
    Task<IEnumerable<Exam>> GetByTeacherIdAsync(Guid teacherId, CancellationToken ct = default);
    Task<IEnumerable<Exam>> GetUpcomingExamsAsync(Guid classId, CancellationToken ct = default);
    Task<bool> HasExamOnDateAsync(Guid classId, DateOnly examDate, CancellationToken ct = default);

    // Advanced filtering
    Task<IEnumerable<Exam>> GetByStatusAsync(Guid classId, string status, CancellationToken ct = default);
    Task<IEnumerable<Exam>> GetByTypeAsync(Guid classId, string examType, CancellationToken ct = default);
    Task<IEnumerable<Exam>> GetByDateRangeAsync(Guid classId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default);
    Task<IEnumerable<Exam>> GetPastExamsAsync(Guid classId, CancellationToken ct = default);

    // Teacher specific
    Task<IEnumerable<Exam>> GetTeacherUpcomingExamsAsync(Guid teacherId, CancellationToken ct = default);
    Task<IEnumerable<Exam>> GetTeacherExamsByStatusAsync(Guid teacherId, string status, CancellationToken ct = default);

    // Statistics and counts
    Task<int> CountByClassAsync(Guid classId, CancellationToken ct = default);
    Task<int> CountByStatusAsync(Guid classId, string status, CancellationToken ct = default);
    Task<int> CountUpcomingExamsAsync(Guid classId, CancellationToken ct = default);

    // Single query optimizations
    Task<Exam?> GetWithDetailsAsync(Guid examId, CancellationToken ct = default);
    Task<Exam?> GetWithResultsAsync(Guid examId, CancellationToken ct = default);

    // Validation helpers
    Task<bool> ExistsForClassAsync(Guid classId, Guid examId, CancellationToken ct = default);
    Task<bool> IsTeacherOwnerAsync(Guid examId, Guid teacherId, CancellationToken ct = default);
}

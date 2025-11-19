using LMS.Models.Entities;

namespace LMS.Services.Interfaces.TeacherService;

public interface IExamResultService : ICrudService<ExamResult, long>
{
    Task<IEnumerable<ExamResult>> GetResultsByExamAsync(Guid examId, CancellationToken ct = default);
    Task<ExamResult?> GetStudentExamResultAsync(Guid examId, Guid studentId, CancellationToken ct = default);
    Task<ExamResult> SubmitResultAsync(ExamResult result, CancellationToken ct = default);
    Task<ExamResult> UpdateResultAsync(ExamResult result, CancellationToken ct = default);
    Task<bool> DeleteResultAsync(long resultId, CancellationToken ct = default);
    Task<bool> CanSubmitResultAsync(Guid examId, Guid studentId, CancellationToken ct = default);
    Task<bool> IsStudentInExamClassAsync(Guid examId, Guid studentId, CancellationToken ct = default);
    Task<decimal> GetAverageScoreAsync(Guid examId, CancellationToken ct = default);
    Task<int> GetPassCountAsync(Guid examId, decimal passingScore, CancellationToken ct = default);
}

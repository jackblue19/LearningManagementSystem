using LMS.Models.Entities;

namespace LMS.Services.Interfaces.TeacherService;

public interface IExamService : ICrudService<Exam, Guid>
{
    Task<IEnumerable<Exam>> GetExamsByClassAsync(Guid classId, CancellationToken ct = default);
    Task<IEnumerable<Exam>> GetExamsByTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<IEnumerable<Exam>> GetUpcomingExamsAsync(Guid classId, CancellationToken ct = default);
    Task<bool> CanScheduleExamAsync(Guid classId, DateOnly examDate, CancellationToken ct = default);
    Task<bool> PublishExamAsync(Guid examId, CancellationToken ct = default);
}

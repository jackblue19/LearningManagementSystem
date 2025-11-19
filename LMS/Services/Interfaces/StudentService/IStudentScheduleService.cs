using LMS.Models.Entities;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentScheduleService
{
    Task<IReadOnlyList<ClassSchedule>> GetScheduleAsync(
        Guid studentId,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken ct = default);
}

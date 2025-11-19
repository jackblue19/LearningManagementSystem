using LMS.Models.Entities;

namespace LMS.Services.Interfaces.TeacherService;

public interface IClassScheduleService
{
    Task<ClassSchedule?> GetScheduleByIdAsync(
        long scheduleId,
        CancellationToken ct = default);

    Task<IReadOnlyList<ClassSchedule>> GetSchedulesByClassIdAsync(
        Guid classId,
        CancellationToken ct = default);
}

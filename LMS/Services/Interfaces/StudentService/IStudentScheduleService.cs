using LMS.Models.ViewModels.StudentService;

using LMS.Models.Entities;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentScheduleService
{
    Task<IReadOnlyList<StudentScheduleItemVm>> GetScheduleAsync(
           Guid studentId, DateOnly from, DateOnly to, CancellationToken ct = default);
           
    Task<IReadOnlyList<ClassSchedule>> GetScheduleAsync(
        Guid studentId,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken ct = default);
}

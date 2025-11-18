using LMS.Models.ViewModels.StudentService;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentScheduleService
{
    Task<IReadOnlyList<StudentScheduleItemVm>> GetScheduleAsync(
           Guid studentId, DateOnly from, DateOnly to, CancellationToken ct = default);
}

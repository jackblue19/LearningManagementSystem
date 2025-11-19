using LMS.Models.Entities;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentCourseService
{
    Task<IReadOnlyList<Class>> GetRegisteredClassesAsync(
        Guid studentId,
        bool includeCancelled = false,
        CancellationToken ct = default);
}

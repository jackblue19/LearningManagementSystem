using LMS.Models.Entities;

namespace LMS.Services.Interfaces.TeacherService;

public interface IClassManagementService
{
    Task<IReadOnlyList<Class>> GetClassesByTeacherIdAsync(
        Guid teacherId,
        CancellationToken ct = default);

    Task<Class?> GetClassByIdAsync(
        Guid classId,
        CancellationToken ct = default);
}

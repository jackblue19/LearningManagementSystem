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
    Task<IReadOnlyList<Class>> GetClassesForSchedulingAsync(
        Guid teacherId,
        CancellationToken ct = default);
    Task AssignTeacherAsync(
        Guid classId,
        Guid teacherId,
        CancellationToken ct = default);
    Task<IReadOnlyList<Subject>> GetSubjectsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Center>> GetCentersAsync(CancellationToken ct = default);
    Task<Class> CreateSelfManagedClassAsync(
        Guid teacherId,
        Guid centerId,
        long subjectId,
        string className,
        string? classAddress = null,
        CancellationToken ct = default);
}

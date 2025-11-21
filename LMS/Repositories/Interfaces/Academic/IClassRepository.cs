using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Academic;

public interface IClassRepository : IGenericRepository<Class, Guid>
{
    Task<IReadOnlyList<Class>> GetClassesByTeacherIdAsync(
        Guid teacherId,
        CancellationToken ct = default);
    Task<IEnumerable<Class>> GetByTeacherIdAsync(Guid teacherId, CancellationToken ct = default);
    Task<IEnumerable<Class>> GetActiveClassesByTeacherAsync(Guid teacherId, CancellationToken ct = default);
    Task<IReadOnlyList<Class>> GetClassesForSchedulingAsync(
        Guid teacherId,
        CancellationToken ct = default);
    Task AssignTeacherAsync(
        Guid classId,
        Guid teacherId,
        CancellationToken ct = default);
}

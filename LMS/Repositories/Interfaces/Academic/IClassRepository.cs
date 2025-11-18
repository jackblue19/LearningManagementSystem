using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Academic;

public interface IClassRepository : IGenericRepository<Class, Guid>
{
    Task<IReadOnlyList<Class>> GetClassesByTeacherIdAsync(
        Guid teacherId,
        CancellationToken ct = default);
}

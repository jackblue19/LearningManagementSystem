using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Academic;

public interface IClassRepository : IGenericRepository<Class, Guid>
{
    Task<IEnumerable<Class>> GetByTeacherIdAsync(Guid teacherId, CancellationToken ct = default);
    Task<IEnumerable<Class>> GetActiveClassesByTeacherAsync(Guid teacherId, CancellationToken ct = default);
}

using LMS.Models.Entities;
using LMS.Repositories;

namespace LMS.Repositories.Interfaces.Academic;

public interface IClassRegistrationRepository : IGenericRepository<ClassRegistration, long>
{
    Task<IEnumerable<ClassRegistration>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default);
    Task<IEnumerable<ClassRegistration>> GetByClassIdAsync(Guid classId, CancellationToken ct = default);
}

using LMS.Models.Entities;

namespace LMS.Services.Interfaces.ManagerService;

public interface IClassService
{
    Task<List<Class>> GetAllClassesAsync(CancellationToken ct = default);
    Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken ct = default);
}

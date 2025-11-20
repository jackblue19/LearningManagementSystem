using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class ClassManagementService : IClassManagementService
{
    private readonly IClassRepository _classRepo;

    public ClassManagementService(IClassRepository classRepo)
    {
        _classRepo = classRepo;
    }

    public async Task<IReadOnlyList<Class>> GetClassesByTeacherIdAsync(
        Guid teacherId,
        CancellationToken ct = default)
    {
        return await _classRepo.GetClassesByTeacherIdAsync(teacherId, ct);
    }

    public async Task<Class?> GetClassByIdAsync(
        Guid classId,
        CancellationToken ct = default)
    {
        return await _classRepo.GetByIdAsync(classId, asNoTracking: true, ct);
    }
}

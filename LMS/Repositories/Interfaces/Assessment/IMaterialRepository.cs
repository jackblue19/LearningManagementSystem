using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Assessment;

public interface IMaterialRepository : IGenericRepository<ClassMaterial, Guid>
{
    Task<IReadOnlyList<ClassMaterial>> GetMaterialsByClassIdAsync(
        Guid classId, 
        CancellationToken ct = default);
    
    Task<IReadOnlyList<ClassMaterial>> GetMaterialsByTeacherIdAsync(
        Guid teacherId, 
        CancellationToken ct = default);
}

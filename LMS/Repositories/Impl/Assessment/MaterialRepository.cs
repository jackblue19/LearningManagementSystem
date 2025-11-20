using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Assessment;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Assessment;

public class MaterialRepository : GenericRepository<ClassMaterial, Guid>, IMaterialRepository
{
    private readonly CenterDbContext _db;

    public MaterialRepository(CenterDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ClassMaterial>> GetMaterialsByClassIdAsync(
        Guid classId, 
        CancellationToken ct = default)
    {
        return await _db.ClassMaterials
            .AsNoTracking()
            .Where(m => m.ClassId == classId)
            .Include(m => m.UploadedByUser)
            .Include(m => m.Class)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ClassMaterial>> GetMaterialsByTeacherIdAsync(
        Guid teacherId, 
        CancellationToken ct = default)
    {
        return await _db.ClassMaterials
            .AsNoTracking()
            .Where(m => m.Class.TeacherId == teacherId)
            .Include(m => m.UploadedByUser)
            .Include(m => m.Class)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync(ct);
    }
}

using LMS.Data;
using LMS.Models.Entities;
using LMS.Services.Interfaces.ManagerService;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services.Impl.ManagerService;

public class ClassService : IClassService
{
    private readonly CenterDbContext _db;

    public ClassService(CenterDbContext db)
    {
        _db = db;
    }

    public async Task<List<Class>> GetAllClassesAsync(CancellationToken ct = default)
    {
        return await _db.Classes
            .OrderBy(c => c.ClassName)
            .ToListAsync(ct);
    }

    public async Task<Class?> GetClassByIdAsync(Guid classId, CancellationToken ct = default)
    {
        return await _db.Classes
            .FirstOrDefaultAsync(c => c.ClassId == classId, ct);
    }
}

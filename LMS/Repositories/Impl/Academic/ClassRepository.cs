using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Academic;

public class ClassRepository : GenericRepository<Class, Guid>, IClassRepository
{
    public ClassRepository(CenterDbContext db) : base(db) { }

    public async Task<IEnumerable<Class>> GetByTeacherIdAsync(Guid teacherId, CancellationToken ct = default)
    {
        return await _db.Set<Class>()
                        .Include(c => c.Subject)
                        .Include(c => c.Center)
                        .Where(c => c.TeacherId == teacherId)
                        .OrderByDescending(c => c.CreatedAt)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<Class>> GetActiveClassesByTeacherAsync(Guid teacherId, CancellationToken ct = default)
    {
        return await _db.Set<Class>()
                        .Include(c => c.Subject)
                        .Where(c => c.TeacherId == teacherId && c.ClassStatus == "Active")
                        .OrderBy(c => c.ClassName)
                        .ToListAsync(ct);
    }
    private readonly CenterDbContext _db;

    public ClassRepository(CenterDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Class>> GetClassesByTeacherIdAsync(
        Guid teacherId,
        CancellationToken ct = default)
    {
        return await _db.Classes
            .AsNoTracking()
            .Where(c => c.TeacherId == teacherId)
            .Include(c => c.Subject)
            .Include(c => c.Center)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);
    }
}

using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Academic;

public class ClassRepository : GenericRepository<Class, Guid>, IClassRepository
{
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

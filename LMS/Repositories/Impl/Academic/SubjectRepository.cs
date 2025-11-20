using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Academic;

public class SubjectRepository : GenericRepository<Subject, long>, ISubjectRepository
{
    public SubjectRepository(CenterDbContext db) : base(db) { }

    public async Task<IEnumerable<Subject>> GetByDepartmentIdAsync(Guid centerId, CancellationToken ct = default)
    {
        return await _db.Set<Subject>()
                        .Where(s => s.CenterId == centerId)
                        .OrderBy(s => s.SubjectName)
                        .ToListAsync(ct);
    }

    public async Task<bool> HasSubjectAsync(string subjectName, CancellationToken ct = default)
    {
        return await _db.Set<Subject>()
                        .AnyAsync(s => s.SubjectName == subjectName, ct);
    }

    public async Task<Subject> GetSubjectByNameAsync(string subjectName, CancellationToken ct = default)
    {
        return await _db.Set<Subject>()
                        .FirstOrDefaultAsync(s => s.SubjectName == subjectName, ct);
    }

}

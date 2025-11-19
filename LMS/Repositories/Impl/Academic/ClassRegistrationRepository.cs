using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Academic;

public class ClassRegistrationRepository : GenericRepository<ClassRegistration, long>, IClassRegistrationRepository
{
    public ClassRegistrationRepository(CenterDbContext db) : base(db) { }

    public async Task<IEnumerable<ClassRegistration>> GetByStudentIdAsync(Guid studentId, CancellationToken ct = default)
    {
        return await _db.Set<ClassRegistration>()
                        .Include(cr => cr.Class)
                        .Where(cr => cr.StudentId == studentId && cr.RegistrationStatus == "Active")
                        .OrderByDescending(cr => cr.RegisteredAt)
                        .ToListAsync(ct);
    }

    public async Task<IEnumerable<ClassRegistration>> GetByClassIdAsync(Guid classId, CancellationToken ct = default)
    {
        return await _db.Set<ClassRegistration>()
                        .Include(cr => cr.Student)
                        .Where(cr => cr.ClassId == classId)
                        .OrderBy(cr => cr.Student.FullName)
                        .ToListAsync(ct);
    }
}

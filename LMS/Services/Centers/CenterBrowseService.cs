using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Repositories;
using LMS.Services.Interfaces;

namespace LMS.Services.Centers;
public sealed class CenterBrowseService : ICenterBrowseService
{
    private readonly IGenericRepository<Center, Guid> _centers;
    private readonly IGenericRepository<Subject, long> _subjects;
    private readonly IGenericRepository<User, Guid> _users;
    private readonly ICrudService<Class, Guid> _classes; 

    public CenterBrowseService(
        IGenericRepository<Center, Guid> centers,
        IGenericRepository<Subject, long> subjects,
        IGenericRepository<User, Guid> users,
        ICrudService<Class, Guid> classes)
    {
        _centers = centers;
        _subjects = subjects;
        _users = users;
        _classes = classes;
    }

    public Task<IReadOnlyList<Center>> ListCentersAsync(CancellationToken ct = default)
        => _centers.ListAsync(asNoTracking: true, ct: ct);

    public Task<Center?> GetCenterAsync(Guid centerId, CancellationToken ct = default)
        => _centers.GetByIdAsync(centerId, asNoTracking: true, ct: ct);

    public Task<IReadOnlyList<Subject>> ListSubjectsByCenterAsync(Guid centerId, CancellationToken ct = default)
        => _subjects.ListAsync(s => s.CenterId == centerId, asNoTracking: true, ct: ct);

    public Task<IReadOnlyList<User>> ListTeachersAsync(CancellationToken ct = default)
        => _users.ListAsync(u => u.RoleDesc == "teacher", asNoTracking: true, ct: ct);

    public Task<PagedResult<Class>> PagedClassesAsync(
        Guid centerId,
        long? subjectId,
        Guid? teacherId,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default)
        => _classes.ListAsync(
            predicate: c => c.CenterId == centerId
                         && (subjectId == null || c.SubjectId == subjectId)
                         && (teacherId == null || c.TeacherId == teacherId),
            orderBy: q => q.OrderBy(c => c.ClassId),
            pageIndex: pageIndex,
            pageSize: pageSize,
            asNoTracking: true,
            includes: null,
            ct: ct);
}

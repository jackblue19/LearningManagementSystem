using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.StudentService;
using LMS.Repositories;
using LMS.Services.Interfaces.StudentService;
using System.Linq;
using LMS.Repositories.Interfaces.Academic;

namespace LMS.Services.Impl.StudentService;

public class StudentCourseService : IStudentCourseService
{
    private readonly IGenericRepository<Class, Guid> _classRepo;
    private readonly IGenericRepository<ClassRegistration, long> _regRepo;

    public StudentCourseService(
        IGenericRepository<Class, Guid> classRepo,
        IGenericRepository<ClassRegistration, long> regRepo)
    {
        _classRepo = classRepo;
        _regRepo = regRepo;
    }

    public async Task<PagedResult<StudentCourseListItemVm>> ListAvailableClassesAsync(
        Guid studentId, string? search = null, Guid? centerId = null, long? subjectId = null,
        int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;
        var skip = (pageIndex - 1) * pageSize;

        Expression<Func<Class, bool>> pred = c =>
            (string.IsNullOrEmpty(search) || c.ClassName.Contains(search)) &&
            (!centerId.HasValue || c.CenterId == centerId.Value) &&
            (!subjectId.HasValue || c.SubjectId == subjectId.Value);

        var classes = await _classRepo.ListAsync(
                        predicate: pred,
                        orderBy: q => q.OrderBy(c => c.ClassName),
                        skip: skip, take: pageSize,
                        asNoTracking: true,
                        includes: new Expression<Func<Class, object>>[]
                                    { c => c.Subject! },
                        ct: ct);

        var total = await _classRepo.CountAsync(pred, ct);

        var clsIds = classes.Select(c => c.ClassId).ToArray();
        var regs = await _regRepo.ListAsync(
            r => r.StudentId == studentId && clsIds.Contains(r.ClassId)
                                          && r.RegistrationStatus == "approved",
                                asNoTracking: true, ct: ct);

        var registered = regs.Select(r => r.ClassId).ToHashSet();
        var items = classes
            .Where(c => !registered.Contains(c.ClassId))
            .Select(c => new StudentCourseListItemVm(
                c.ClassId, c.ClassName, c.CenterId, c.SubjectId, c.Subject?.SubjectName,
                c.UnitPrice, c.TotalSessions, c.ScheduleDesc, c.ClassStatus))
            .ToList();

        return new PagedResult<StudentCourseListItemVm>(items, total, pageIndex, pageSize);
    }

    public async Task<PagedResult<StudentCourseListItemVm>> ListMyClassesAsync(
       Guid studentId, int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;
        var skip = (pageIndex - 1) * pageSize;

        var regs = await _regRepo.ListAsync(
            predicate: r => r.StudentId == studentId && r.RegistrationStatus == "approved",
            orderBy: q => q.OrderByDescending(r => r.RegisteredAt),
            skip: skip, take: pageSize,
            asNoTracking: true,
            includes: new Expression<Func<ClassRegistration, object>>[]
                        { r => r.Class!, r => r.Class!.Subject! },
            ct: ct);

        var total = await _regRepo.CountAsync(r => r.StudentId == studentId && r.RegistrationStatus == "approved", ct);

        var items = regs.Select(r =>
        {
            var c = r.Class!;
            return new StudentCourseListItemVm(
                c.ClassId, c.ClassName, c.CenterId, c.SubjectId, c.Subject?.SubjectName,
                c.UnitPrice, c.TotalSessions, c.ScheduleDesc, c.ClassStatus);
        }).ToList();

        return new PagedResult<StudentCourseListItemVm>(items, total, pageIndex, pageSize);
    }

    private const string StatusCancelled = "Cancelled";


    public async Task<IReadOnlyList<Class>> GetRegisteredClassesAsync(
        Guid studentId,
        bool includeCancelled = false,
        CancellationToken ct = default)
    {
        Expression<Func<ClassRegistration, bool>> predicate = registration =>
            registration.StudentId == studentId &&
            (includeCancelled ||
             !string.Equals(registration.RegistrationStatus, StatusCancelled, StringComparison.OrdinalIgnoreCase));

        var includes = new Expression<Func<ClassRegistration, object>>[]
        {
            registration => registration.Class
        };

        var registrations = await _regRepo.ListAsync(
            predicate: predicate,
            includes: includes,
            ct: ct);

        return registrations
            .Where(reg => reg.Class is not null)
            .Select(reg => reg.Class)
            .ToList();
    }
}

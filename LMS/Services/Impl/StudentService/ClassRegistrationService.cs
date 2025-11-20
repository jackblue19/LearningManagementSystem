using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.StudentService;
using LMS.Repositories;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Services.Impl.StudentService;

public class ClassRegistrationService : IClassRegistrationService
{
    private readonly IGenericRepository<ClassRegistration, Guid> _regRepo;
    private readonly IGenericRepository<Class, Guid> _classRepo;
    public ClassRegistrationService(
            IGenericRepository<ClassRegistration, Guid> regRepo,
            IGenericRepository<Class, Guid> classRepo)
    {
        _regRepo = regRepo;
        _classRepo = classRepo;
    }

    public async Task<bool> RegisterAsync(Guid studentId, Guid classId, CancellationToken ct = default)
    {
        var dup = await _regRepo.ExistsAsync(r =>
            r.StudentId == studentId && r.ClassId == classId
                                     && r.RegistrationStatus == "approved", ct);
        if (dup) return false;

        // Check exist
        var cls = await _classRepo.GetByIdAsync(classId, asNoTracking: true, ct);
        if (cls is null) return false;

        var reg = new ClassRegistration
        {
            StudentId = studentId,
            ClassId = classId,
            RegisteredAt = DateTime.UtcNow,
            RegistrationStatus = "approved"
        };

        await _regRepo.AddAsync(reg, saveNow: false, ct);
        await _regRepo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> CancelAsync(Guid studentId, Guid classId, CancellationToken ct = default)
    {
        var reg = await _regRepo.FirstOrDefaultAsync(
            r => r.StudentId == studentId && r.ClassId == classId
                                          && r.RegistrationStatus == "approved",
                                asNoTracking: false, includes: null, ct: ct);
        if (reg is null) return false;

        reg.RegistrationStatus = "rejected";
        await _regRepo.UpdateAsync(reg, saveNow: false, ct);
        await _regRepo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<PagedResult<StudentRegisteredClassVm>> ListMyRegistrationsAsync(
        Guid studentId, int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;
        var skip = (pageIndex - 1) * pageSize;

        var regs = await _regRepo.ListAsync(
                    predicate: r => r.StudentId == studentId,
                    orderBy: q => q.OrderByDescending(r => r.RegisteredAt),
                    skip: skip, take: pageSize,
                    asNoTracking: true,
                    includes: new Expression<Func<ClassRegistration, object>>[]
                                { r => r.Class! },
                    ct: ct);

        var total = await _regRepo.CountAsync(r => r.StudentId == studentId, ct);

        var items = regs.Select(r => new StudentRegisteredClassVm(
            r.RegistrationId, r.ClassId, r.Class?.ClassName ?? "",
            r.Class?.StartDate, r.Class?.EndDate, r.Class?.ScheduleDesc,
            r.RegistrationStatus, r.RegisteredAt)).ToList();

        return new PagedResult<StudentRegisteredClassVm>(items, total, pageIndex, pageSize);
    }


}

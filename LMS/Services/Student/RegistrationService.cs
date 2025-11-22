using LMS.Models.Entities;
using LMS.Repositories;

namespace LMS.Services.Student;

public sealed class RegistrationService : IRegistrationService
{
    private readonly IGenericRepository<ClassRegistration, long> _regs;
    private readonly IGenericRepository<Class, Guid> _classes;

    public RegistrationService(
        IGenericRepository<ClassRegistration, long> regs,
        IGenericRepository<Class, Guid> classes)
    {
        _regs = regs;
        _classes = classes;
    }

    public async Task<bool> CanRegisterAsync(Guid studentId, Guid classId, CancellationToken ct = default)
    {
        var cls = await _classes.GetByIdAsync(classId, asNoTracking: true, ct: ct);
        if (cls is null) return false;

        var existsActive = await _regs.ExistsAsync(
            r => r.ClassId == classId
              && r.StudentId == studentId
              && (r.RegistrationStatus == "Pending" || r.RegistrationStatus == "Approved"),
            ct);

        return !existsActive;
    }

    public async Task<ClassRegistration> RegisterAsync(Guid studentId, Guid classId, string? note, CancellationToken ct = default)
    {
        var reg = new ClassRegistration
        {
            ClassId = classId,
            StudentId = studentId,
            RegistrationStatus = "Approved",
            RegisteredAt = DateTime.UtcNow,
        };

        await _regs.AddAsync(reg, saveNow: true, ct);
        return reg;
    }
}

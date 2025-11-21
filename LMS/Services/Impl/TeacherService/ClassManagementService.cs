using System.Linq;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Info;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class ClassManagementService : IClassManagementService
{
    private readonly IClassRepository _classRepo;
    private readonly ISubjectRepository _subjectRepo;
    private readonly ICenterRepository _centerRepo;

    public ClassManagementService(
        IClassRepository classRepo,
        ISubjectRepository subjectRepo,
        ICenterRepository centerRepo)
    {
        _classRepo = classRepo;
        _subjectRepo = subjectRepo;
        _centerRepo = centerRepo;
    }

    public async Task<IReadOnlyList<Class>> GetClassesByTeacherIdAsync(
        Guid teacherId,
        CancellationToken ct = default)
    {
        return await _classRepo.GetClassesByTeacherIdAsync(teacherId, ct);
    }

    public async Task<Class?> GetClassByIdAsync(
        Guid classId,
        CancellationToken ct = default)
    {
        return await _classRepo.GetByIdAsync(classId, asNoTracking: true, ct);
    }

    public async Task<IReadOnlyList<Class>> GetClassesForSchedulingAsync(
        Guid teacherId,
        CancellationToken ct = default)
    {
        return await _classRepo.GetClassesForSchedulingAsync(teacherId, ct);
    }

    public async Task AssignTeacherAsync(
        Guid classId,
        Guid teacherId,
        CancellationToken ct = default)
    {
        await _classRepo.AssignTeacherAsync(classId, teacherId, ct);
    }

    public async Task<IReadOnlyList<Subject>> GetSubjectsAsync(CancellationToken ct = default)
    {
        return await _subjectRepo.ListAsync(
            orderBy: q => q.OrderBy(s => s.SubjectName),
            asNoTracking: true,
            ct: ct);
    }

    public async Task<IReadOnlyList<Center>> GetCentersAsync(CancellationToken ct = default)
    {
        return await _centerRepo.ListAsync(
            orderBy: q => q.OrderBy(c => c.CenterName),
            asNoTracking: true,
            ct: ct);
    }

    public async Task<Class> CreateSelfManagedClassAsync(
        Guid teacherId,
        Guid centerId,
        long subjectId,
        string className,
        string? classAddress = null,
        CancellationToken ct = default)
    {
        if (teacherId == Guid.Empty)
        {
            throw new ArgumentException("Teacher id is required.", nameof(teacherId));
        }

        if (centerId == Guid.Empty)
        {
            throw new ArgumentException("Center id is required.", nameof(centerId));
        }

        if (string.IsNullOrWhiteSpace(className))
        {
            throw new ArgumentException("Class name is required.", nameof(className));
        }

        var subject = await _subjectRepo.GetByIdAsync(subjectId, asNoTracking: true, ct);
        if (subject is null)
        {
            throw new InvalidOperationException("Selected subject does not exist anymore.");
        }

        var center = await _centerRepo.GetByIdAsync(centerId, asNoTracking: true, ct);
        if (center is null)
        {
            throw new InvalidOperationException("Selected center does not exist anymore.");
        }

        var normalizedName = className.Trim();

        var entity = new Class
        {
            ClassId = Guid.NewGuid(),
            ClassName = normalizedName,
            SubjectId = subjectId,
            TeacherId = teacherId,
            CenterId = centerId,
            ClassAddress = classAddress,
            ClassStatus = "active",
            CreatedAt = DateTime.UtcNow,
            ScheduleDesc = "Self-managed schedule",
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };

        return await _classRepo.AddAsync(entity, saveNow: true, ct);
    }
}

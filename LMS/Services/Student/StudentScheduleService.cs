using LMS.Models.Entities;
using LMS.Repositories;

namespace LMS.Services.Student;

public sealed class StudentScheduleService : IStudentScheduleService
{
    private readonly IGenericRepository<ClassRegistration, long> _regs;
    private readonly IGenericRepository<ClassSchedule, long> _schedules;
    private readonly IGenericRepository<Class, Guid> _classes;

    public StudentScheduleService(
        IGenericRepository<ClassRegistration, long> regs,
        IGenericRepository<ClassSchedule, long> schedules,
        IGenericRepository<Class, Guid> classes)
    {
        _regs = regs;
        _schedules = schedules;
        _classes = classes;
    }

    public async Task<IReadOnlyList<StudentScheduleItem>> ListSchedulesByStudentAsync(
        Guid studentId,
        DateOnly? from,
        DateOnly? to,
        Guid? classId,
        CancellationToken ct = default)
    {
        var approvedRegs = await _regs.ListAsync(
            r => r.StudentId == studentId
              && r.RegistrationStatus == "Approved"
              && (classId == null || r.ClassId == classId),
            asNoTracking: true,
            ct: ct);

        if (approvedRegs.Count == 0)
            return Array.Empty<StudentScheduleItem>();

        var classIds = approvedRegs.Select(r => r.ClassId).Distinct().ToList();

        var schedules = await _schedules.ListAsync(
            s => classIds.Contains(s.ClassId)
              && (from == null || s.SessionDate >= from.Value)
              && (to == null || s.SessionDate <= to.Value),
            orderBy: q => q.OrderBy(s => s.SessionDate).ThenBy(s => s.SlotOrder),
            asNoTracking: true,
            ct: ct);

        if (schedules.Count == 0)
            return Array.Empty<StudentScheduleItem>();

        var classMap = (await _classes.ListAsync(
                c => classIds.Contains(c.ClassId),
                asNoTracking: true,
                ct: ct))
            .ToDictionary(c => c.ClassId, c => c.ClassName ?? string.Empty);

        return schedules.Select(s => new StudentScheduleItem(
            ScheduleId: s.ScheduleId,
            ClassId: s.ClassId,
            ClassName: classMap.TryGetValue(s.ClassId, out var cn) ? cn : string.Empty,
            SessionDate: s.SessionDate,
            StartTime: s.StartTime?.ToString(),
            EndTime: s.EndTime?.ToString(),
            RoomName: s.RoomName,
            SlotOrder: s.SlotOrder,
            Label: s.ScheduleLabel,
            Note: s.ScheduleNote
        )).ToList();
    }

    public async Task<IReadOnlyList<MyClassItem>> ListMyClassesAsync(Guid studentId, CancellationToken ct = default)
    {
        var regs = await _regs.ListAsync(
            r => r.StudentId == studentId && r.RegistrationStatus == "Approved",
            asNoTracking: true,
            ct: ct);

        if (regs.Count == 0) return Array.Empty<MyClassItem>();

        var classIds = regs.Select(r => r.ClassId).Distinct().ToList();
        var classes = await _classes.ListAsync(c => classIds.Contains(c.ClassId), asNoTracking: true, ct: ct);

        return classes
            .Select(c => new MyClassItem(c.ClassId, c.ClassName ?? string.Empty))
            .OrderBy(x => x.ClassName)
            .ToList();
    }
}

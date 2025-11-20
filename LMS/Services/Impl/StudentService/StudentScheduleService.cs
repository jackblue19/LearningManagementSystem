using LMS.Models.Entities;
using LMS.Models.ViewModels.StudentService;
using LMS.Repositories;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Services.Impl.StudentService;

public class StudentScheduleService : IStudentScheduleService
{
    private readonly IGenericRepository<ClassRegistration, long> _regRepo;
    private readonly IGenericRepository<ClassSchedule, long> _scheduleRepo;
    private readonly IGenericRepository<Class, Guid> _classRepo;

    public StudentScheduleService(
        IGenericRepository<ClassRegistration, long> regRepo,
        IGenericRepository<ClassSchedule, long> scheduleRepo,
        IGenericRepository<Class, Guid> classRepo)
    {
        _regRepo = regRepo;
        _scheduleRepo = scheduleRepo;
        _classRepo = classRepo;
    }

    public async Task<IReadOnlyList<StudentScheduleItemVm>> GetScheduleAsync(
       Guid studentId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        var regs = await _regRepo.ListAsync(
            r => r.StudentId == studentId && r.RegistrationStatus == "approved",
                                asNoTracking: true, ct: ct);

        var clsIds = regs.Select(r => r.ClassId).Distinct().ToArray();
        if (clsIds.Length == 0)
            return Array.Empty<StudentScheduleItemVm>();

        var schedules = await _scheduleRepo.ListAsync(
            s => clsIds.Contains(s.ClassId) && s.SessionDate >= from && s.SessionDate <= to,
            orderBy: q => q.OrderBy(s => s.SessionDate).ThenBy(s => s.SlotOrder),
            asNoTracking: true, ct: ct);

        var classes = await _classRepo.ListAsync(c => clsIds.Contains(c.ClassId), asNoTracking: true, ct: ct);
        var classMap = classes.ToDictionary(c => c.ClassId, c => c.ClassName);

        return schedules.Select(s => new StudentScheduleItemVm(
                                        s.ScheduleId, s.ClassId,
                                        classMap.TryGetValue(s.ClassId, out var name)
                                            ? name
                                            : "",
                                        s.SessionDate, s.StartTime, s.EndTime,
                                        s.SlotOrder, s.RoomName, s.ScheduleLabel))
                        .ToList();
    }
}

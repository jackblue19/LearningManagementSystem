using LMS.Models.Entities;
using LMS.Models.ViewModels.StudentService;
using LMS.Repositories;
using LMS.Services.Interfaces.StudentService;
using System.Linq;
using System.Linq.Expressions;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Scheduling;

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

    public async Task<IReadOnlyList<StudentScheduleItemVm>> GetScheduleAsyncZ(
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

    private const string StatusCancelled = "Cancelled";

    public async Task<IReadOnlyList<ClassSchedule>> GetScheduleAsync(
        Guid studentId,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken ct = default)
    {
        var registrations = await _regRepo.ListAsync(
            predicate: registration =>
                registration.StudentId == studentId &&
                !string.Equals(registration.RegistrationStatus, StatusCancelled, StringComparison.OrdinalIgnoreCase),
            ct: ct);

        var classIds = registrations
            .Select(reg => reg.ClassId)
            .Distinct()
            .ToList();

        if (classIds.Count == 0) return Array.Empty<ClassSchedule>();

        Expression<Func<ClassSchedule, bool>> predicate = schedule =>
            classIds.Contains(schedule.ClassId) &&
            (!startDate.HasValue || schedule.SessionDate >= startDate.Value) &&
            (!endDate.HasValue || schedule.SessionDate <= endDate.Value);

        var includes = new Expression<Func<ClassSchedule, object>>[]
        {
            schedule => schedule.Class,
            schedule => schedule.Class.Teacher,
            schedule => schedule.Room
        };

        var schedules = await _scheduleRepo.ListAsync(
            predicate: predicate,
            orderBy: query => query
                .OrderBy(schedule => schedule.SessionDate)
                .ThenBy(schedule => schedule.StartTime),
            includes: includes,
            ct: ct);

        return schedules;
    }
}

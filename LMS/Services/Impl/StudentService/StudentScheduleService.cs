using System.Linq;
using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Services.Impl.StudentService;

public class StudentScheduleService : IStudentScheduleService
{
    private readonly IClassRegistrationRepository _registrationRepository;
    private readonly IClassScheduleRepository _scheduleRepository;

    private const string StatusCancelled = "Cancelled";

    public StudentScheduleService(
        IClassRegistrationRepository registrationRepository,
        IClassScheduleRepository scheduleRepository)
    {
        _registrationRepository = registrationRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IReadOnlyList<ClassSchedule>> GetScheduleAsync(
        Guid studentId,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken ct = default)
    {
        var registrations = await _registrationRepository.ListAsync(
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

        var schedules = await _scheduleRepository.ListAsync(
            predicate: predicate,
            orderBy: query => query
                .OrderBy(schedule => schedule.SessionDate)
                .ThenBy(schedule => schedule.StartTime),
            includes: includes,
            ct: ct);

        return schedules;
    }
}

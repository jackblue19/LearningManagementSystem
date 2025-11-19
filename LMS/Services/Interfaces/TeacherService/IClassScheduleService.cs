using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LMS.Models.Entities;
using LMS.Models.ViewModels.Scheduling;
using LMS.Services.Interfaces;

namespace LMS.Services.Interfaces.TeacherService;

public interface IClassScheduleService : ICrudService<ClassSchedule, long>
{
    Task<ScheduleAvailabilityResult> CheckAvailabilityAsync(
        ClassSchedule schedule,
        CancellationToken ct = default);

    Task<ScheduleOperationResult> ScheduleAsync(
        ClassSchedule schedule,
        bool saveNow = true,
        CancellationToken ct = default);

    Task<IReadOnlyList<ClassSchedule>> GetClassScheduleAsync(
        Guid classId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);
}

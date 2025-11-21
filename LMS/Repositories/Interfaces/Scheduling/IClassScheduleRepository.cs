using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Scheduling;

public interface IClassScheduleRepository : IGenericRepository<ClassSchedule, long>
{
    Task<bool> HasClassConflictAsync(
        Guid classId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        byte? slotId = null,
        long? excludeScheduleId = null,
        CancellationToken ct = default);

    Task<bool> HasTeacherConflictAsync(
        Guid teacherId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        byte? slotId = null,
        long? excludeScheduleId = null,
        CancellationToken ct = default);

    Task<bool> HasRoomConflictAsync(
        Guid roomId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        byte? slotId = null,
        long? excludeScheduleId = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<ClassSchedule>> GetByClassAsync(
        Guid classId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);
}

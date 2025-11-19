using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Scheduling;

public class ClassScheduleRepository : GenericRepository<ClassSchedule, long>, IClassScheduleRepository
{
    public ClassScheduleRepository(CenterDbContext db) : base(db)
    {
    }

    public async Task<bool> HasClassConflictAsync(
        Guid classId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        byte? slotId = null,
        long? excludeScheduleId = null,
        CancellationToken ct = default)
    {
        var query = _db.Set<ClassSchedule>()
            .Where(s => s.ClassId == classId && s.SessionDate == sessionDate);

        if (excludeScheduleId.HasValue)
        {
            query = query.Where(s => s.ScheduleId != excludeScheduleId.Value);
        }

        return await query.AnyAsync(s =>
            (s.StartTime.HasValue && s.EndTime.HasValue && s.StartTime.Value < endTime && startTime < s.EndTime.Value) ||
            (slotId.HasValue && s.SlotId.HasValue && s.SlotId.Value == slotId.Value), ct);
    }

    public async Task<bool> HasTeacherConflictAsync(
        Guid teacherId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        byte? slotId = null,
        long? excludeScheduleId = null,
        CancellationToken ct = default)
    {
        var query = _db.Set<ClassSchedule>()
            .Where(s => s.SessionDate == sessionDate && s.Class.TeacherId == teacherId);

        if (excludeScheduleId.HasValue)
        {
            query = query.Where(s => s.ScheduleId != excludeScheduleId.Value);
        }

        return await query.AnyAsync(s =>
            (s.StartTime.HasValue && s.EndTime.HasValue && s.StartTime.Value < endTime && startTime < s.EndTime.Value) ||
            (slotId.HasValue && s.SlotId.HasValue && s.SlotId.Value == slotId.Value), ct);
    }

    public async Task<bool> HasRoomConflictAsync(
        Guid roomId,
        DateOnly sessionDate,
        TimeOnly startTime,
        TimeOnly endTime,
        byte? slotId = null,
        long? excludeScheduleId = null,
        CancellationToken ct = default)
    {
        var query = _db.Set<ClassSchedule>()
            .Where(s => s.RoomId == roomId && s.SessionDate == sessionDate);

        if (excludeScheduleId.HasValue)
        {
            query = query.Where(s => s.ScheduleId != excludeScheduleId.Value);
        }

        return await query.AnyAsync(s =>
            (s.StartTime.HasValue && s.EndTime.HasValue && s.StartTime.Value < endTime && startTime < s.EndTime.Value) ||
            (slotId.HasValue && s.SlotId.HasValue && s.SlotId.Value == slotId.Value), ct);
    }

    public async Task<IReadOnlyList<ClassSchedule>> GetByClassAsync(
        Guid classId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default)
    {
        var query = _db.Set<ClassSchedule>()
            .Include(s => s.Slot)
            .Include(s => s.Room)
            .Where(s => s.ClassId == classId);

        if (from.HasValue)
        {
            query = query.Where(s => s.SessionDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(s => s.SessionDate <= to.Value);
        }

        return await query
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .ThenBy(s => s.SlotId)
            .ToListAsync(ct);
    }
}

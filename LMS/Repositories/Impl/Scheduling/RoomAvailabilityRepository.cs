using System;
using System.Threading;
using System.Threading.Tasks;
using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Scheduling;

public class RoomAvailabilityRepository : GenericRepository<RoomAvailability, long>, IRoomAvailabilityRepository
{
    public RoomAvailabilityRepository(CenterDbContext db) : base(db)
    {
    }

    public Task<bool> HasAvailabilityWindowAsync(
        Guid roomId,
        byte dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken ct = default)
    {
        if (endTime <= startTime) return Task.FromResult(false);
        return _db.Set<RoomAvailability>()
            .AnyAsync(avail => avail.RoomId == roomId
                               && avail.DayOfWeek == dayOfWeek
                               && avail.StartTime <= startTime
                               && endTime <= avail.EndTime, ct);
    }

    public async Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startTime, DateTime endTime, CancellationToken ct = default)
    {
        if (endTime <= startTime) return false;

        var sessionDate = DateOnly.FromDateTime(startTime);
        var timeStart = TimeOnly.FromDateTime(startTime);
        var timeEnd = TimeOnly.FromDateTime(endTime);
        var dayOfWeek = (byte)startTime.DayOfWeek;

        if (!await HasAvailabilityWindowAsync(roomId, dayOfWeek, timeStart, timeEnd, ct))
        {
            return false;
        }

        var hasScheduleConflict = await _db.Set<ClassSchedule>()
            .AnyAsync(s => s.RoomId == roomId
                           && s.SessionDate == sessionDate
                           && s.StartTime.HasValue
                           && s.EndTime.HasValue
                           && s.StartTime.Value < timeEnd
                           && timeStart < s.EndTime.Value,
                ct);

        return !hasScheduleConflict;
    }
}

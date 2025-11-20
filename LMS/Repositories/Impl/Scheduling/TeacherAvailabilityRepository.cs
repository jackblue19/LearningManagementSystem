using System;
using System.Threading;
using System.Threading.Tasks;
using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Scheduling;

public class TeacherAvailabilityRepository : GenericRepository<TeacherAvailability, long>, ITeacherAvailabilityRepository
{
    public TeacherAvailabilityRepository(CenterDbContext db) : base(db)
    {
    }

    public Task<bool> HasAvailabilityWindowAsync(
        Guid teacherId,
        byte dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken ct = default)
    {
        if (endTime <= startTime) return Task.FromResult(false);
        return _db.Set<TeacherAvailability>()
            .AnyAsync(avail => avail.TeacherId == teacherId
                               && avail.DayOfWeek == dayOfWeek
                               && avail.StartTime <= startTime
                               && endTime <= avail.EndTime, ct);
    }
}

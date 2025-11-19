using System;
using System.Threading;
using System.Threading.Tasks;
using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Scheduling;

public interface ITeacherAvailabilityRepository : IGenericRepository<TeacherAvailability, long>
{
    Task<bool> HasAvailabilityWindowAsync(
        Guid teacherId,
        byte dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken ct = default);
}

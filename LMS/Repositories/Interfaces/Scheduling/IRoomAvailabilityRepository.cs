using System;
using System.Threading;
using System.Threading.Tasks;
using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Scheduling;

public interface IRoomAvailabilityRepository : IGenericRepository<RoomAvailability, long>
{
    Task<bool> HasAvailabilityWindowAsync(
        Guid roomId,
        byte dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken ct = default);

    Task<bool> IsRoomAvailableAsync(Guid roomId, DateTime startTime, DateTime endTime, CancellationToken ct = default);
}

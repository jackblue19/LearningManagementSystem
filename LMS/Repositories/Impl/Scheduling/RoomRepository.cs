using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Scheduling;

public class RoomRepository : GenericRepository<Room, Guid>, IRoomRepository
{
    private readonly CenterDbContext _db;

    public RoomRepository(CenterDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Room>> GetRoomsByCenterIdAsync(
        Guid centerId, 
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = _db.Rooms
            .AsNoTracking()
            .Where(r => r.CenterId == centerId);

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        return await query
            .Include(r => r.Center)
            .OrderBy(r => r.RoomName)
            .ToListAsync(ct);
    }

    public async Task<Room?> GetRoomByNameAndCenterAsync(
        string roomName, 
        Guid centerId,
        CancellationToken ct = default)
    {
        return await _db.Rooms
            .AsNoTracking()
            .Include(r => r.Center)
            .FirstOrDefaultAsync(r => 
                r.RoomName == roomName && r.CenterId == centerId, ct);
    }

    public async Task<IReadOnlyList<Room>> SearchRoomsAsync(
        string? keyword = null,
        Guid? centerId = null,
        int? minCapacity = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var query = _db.Rooms.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(r => r.RoomName.Contains(keyword));

        if (centerId.HasValue)
            query = query.Where(r => r.CenterId == centerId.Value);

        if (minCapacity.HasValue)
            query = query.Where(r => r.Capacity >= minCapacity.Value);

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        return await query
            .Include(r => r.Center)
            .OrderBy(r => r.Center.CenterName)
            .ThenBy(r => r.RoomName)
            .ToListAsync(ct);
    }
}

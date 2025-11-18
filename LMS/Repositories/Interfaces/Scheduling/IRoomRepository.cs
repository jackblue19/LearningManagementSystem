using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Scheduling;

public interface IRoomRepository : IGenericRepository<Room, Guid>
{
    Task<IReadOnlyList<Room>> GetRoomsByCenterIdAsync(
        Guid centerId, 
        bool? isActive = null,
        CancellationToken ct = default);
    
    Task<Room?> GetRoomByNameAndCenterAsync(
        string roomName, 
        Guid centerId,
        CancellationToken ct = default);
    
    Task<IReadOnlyList<Room>> SearchRoomsAsync(
        string? keyword = null,
        Guid? centerId = null,
        int? minCapacity = null,
        bool? isActive = null,
        CancellationToken ct = default);
}

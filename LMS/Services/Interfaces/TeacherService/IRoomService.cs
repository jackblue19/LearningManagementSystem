using LMS.Models.Entities;

namespace LMS.Services.Interfaces.TeacherService;

public interface IRoomService
{
    Task<IReadOnlyList<Room>> GetAllRoomsAsync(
        bool? isActive = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Room>> GetRoomsByCenterIdAsync(
        Guid centerId,
        bool? isActive = null,
        CancellationToken ct = default);

    Task<Room?> GetRoomByIdAsync(
        Guid roomId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Room>> SearchRoomsAsync(
        string? keyword = null,
        Guid? centerId = null,
        int? minCapacity = null,
        bool? isActive = null,
        CancellationToken ct = default);

    Task<Room?> GetRoomByNameAndCenterAsync(
        string roomName,
        Guid centerId,
        CancellationToken ct = default);
}

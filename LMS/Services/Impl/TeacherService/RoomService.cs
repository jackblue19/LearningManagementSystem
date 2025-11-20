using System;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Impl;
using LMS.Services.Interfaces.TeacherService;
using System.Linq.Expressions;
using LMS.Repositories.Interfaces.Scheduling;

namespace LMS.Services.Impl.TeacherService;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepo;

    public RoomService(IRoomRepository roomRepo)
    {
        _roomRepo = roomRepo;
    }

    public async Task<IReadOnlyList<Room>> GetAllRoomsAsync(
        bool? isActive = null,
        CancellationToken ct = default)
    {
        var includes = new List<Expression<Func<Room, object>>> { r => r.Center };
        
        return await _roomRepo.ListAsync(
            predicate: isActive.HasValue ? r => r.IsActive == isActive.Value : null,
            orderBy: q => q.OrderBy(r => r.RoomName),
            asNoTracking: true,
            includes: includes,
            ct: ct);
    }

    public async Task<IReadOnlyList<Room>> GetRoomsByCenterIdAsync(
        Guid centerId,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        return await _roomRepo.GetRoomsByCenterIdAsync(centerId, isActive, ct);
    }

    public async Task<Room?> GetRoomByIdAsync(
        Guid roomId,
        CancellationToken ct = default)
    {
        return await _roomRepo.GetByIdAsync(roomId, asNoTracking: true, ct);
    }

    public async Task<IReadOnlyList<Room>> SearchRoomsAsync(
        string? keyword = null,
        Guid? centerId = null,
        int? minCapacity = null,
        bool? isActive = null,
        CancellationToken ct = default)
    {
        return await _roomRepo.SearchRoomsAsync(
            keyword, centerId, minCapacity, isActive, ct);
    }

    public async Task<Room?> GetRoomByNameAndCenterAsync(
        string roomName,
        Guid centerId,
        CancellationToken ct = default)
    {
        return await _roomRepo.GetRoomByNameAndCenterAsync(roomName, centerId, ct);
    }
}

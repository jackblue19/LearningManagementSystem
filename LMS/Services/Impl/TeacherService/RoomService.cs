using System;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Impl;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class RoomService : CrudService<Room, Guid>, IRoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
        : base(roomRepository)
    {
        _roomRepository = roomRepository;
    }
}

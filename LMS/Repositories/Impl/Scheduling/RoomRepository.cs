using System;
using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Repositories;

namespace LMS.Repositories.Impl.Scheduling;

public class RoomRepository : GenericRepository<Room, Guid>, IRoomRepository
{
    public RoomRepository(CenterDbContext db) : base(db)
    {
    }
}

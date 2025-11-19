using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;

namespace LMS.Repositories.Impl.Scheduling;

public class RoomAvailabilityRepository : GenericRepository<RoomAvailability, long>, IRoomAvailabilityRepository
{
    public RoomAvailabilityRepository(CenterDbContext db) : base(db)
    {
    }
}

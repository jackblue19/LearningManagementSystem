using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Repositories;

namespace LMS.Repositories.Impl.Scheduling;

public class TimeSlotRepository : GenericRepository<TimeSlot, byte>, ITimeSlotRepository
{
    public TimeSlotRepository(CenterDbContext db) : base(db)
    {
    }
}

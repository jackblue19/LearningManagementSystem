using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;

namespace LMS.Repositories.Impl.Scheduling;

public class ClassScheduleRepository : GenericRepository<ClassSchedule, long>, IClassScheduleRepository
{
    public ClassScheduleRepository(CenterDbContext db) : base(db)
    {
    }
}

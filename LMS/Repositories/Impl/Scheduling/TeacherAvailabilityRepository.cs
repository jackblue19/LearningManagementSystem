using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;

namespace LMS.Repositories.Impl.Scheduling;

public class TeacherAvailabilityRepository : GenericRepository<TeacherAvailability, long>, ITeacherAvailabilityRepository
{
    public TeacherAvailabilityRepository(CenterDbContext db) : base(db)
    {
    }
}

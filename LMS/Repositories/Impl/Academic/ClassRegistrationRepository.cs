using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;

namespace LMS.Repositories.Impl.Academic;

public class ClassRegistrationRepository : GenericRepository<ClassRegistration, long>, IClassRegistrationRepository
{
    public ClassRegistrationRepository(CenterDbContext db) : base(db)
    {
    }
}

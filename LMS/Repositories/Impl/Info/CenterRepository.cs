using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Info;

namespace LMS.Repositories.Impl.Info;

public class CenterRepository : GenericRepository<Center, Guid>, ICenterRepository
{
    public CenterRepository(CenterDbContext db) : base(db)
    {
    }
}

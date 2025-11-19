using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Info;

namespace LMS.Repositories.Impl.Info;

public class UserRepository : GenericRepository<User, Guid>, IUserRepository
{
    public UserRepository(CenterDbContext db) : base(db)
    {
    }
}

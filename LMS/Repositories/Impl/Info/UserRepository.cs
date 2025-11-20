using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Info;

namespace LMS.Repositories.Impl.Info;

public class UserRepository : GenericRepository<User, Guid>, IUserRepository
{
    public UserRepository(LMS.Data.CenterDbContext db) : base(db)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        return await FirstOrDefaultAsync(
            u => u.Username == username,
            asNoTracking: true,
            ct: ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await FirstOrDefaultAsync(
            u => u.Email == email,
            asNoTracking: true,
            ct: ct);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(string roleDesc, CancellationToken ct = default)
    {
        return await ListAsync(
            predicate: u => u.RoleDesc == roleDesc,
            orderBy: q => q.OrderBy(u => u.CreatedAt),
            asNoTracking: true,
            ct: ct);
    }

    public async Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        if (excludeUserId.HasValue)
        {
            return await ExistsAsync(
                u => u.Username == username && u.UserId != excludeUserId.Value,
                ct);
        }
        return await ExistsAsync(u => u.Username == username, ct);
    }

    public async Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken ct = default)
    {
        if (excludeUserId.HasValue)
        {
            return await ExistsAsync(
                u => u.Email == email && u.UserId != excludeUserId.Value,
                ct);
        }
        return await ExistsAsync(u => u.Email == email, ct);
    }
}

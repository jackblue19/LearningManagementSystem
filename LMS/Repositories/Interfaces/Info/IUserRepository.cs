using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Info;

public interface IUserRepository : IGenericRepository<User, Guid>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByRoleAsync(string roleDesc, CancellationToken ct = default);
    Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken ct = default);
    Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken ct = default);
}

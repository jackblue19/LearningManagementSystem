using LMS.Models.Entities;
using LMS.Models.ViewModels;
using System.Linq.Expressions;

namespace LMS.Services.Interfaces.CommonService;

public interface IUserService
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<PagedResult<User>> ListAsync(
        Expression<Func<User, bool>>? predicate = null,
        Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null,
        int pageIndex = 1,
        int pageSize = 50,
        CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetByRoleAsync(string roleDesc, CancellationToken ct = default);
    Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken ct = default);
    Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken ct = default);
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
    Task DeleteAsync(User user, CancellationToken ct = default);
    Task DeleteByIdAsync(Guid id, CancellationToken ct = default);
}

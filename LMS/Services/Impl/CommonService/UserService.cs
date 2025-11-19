using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Repositories.Interfaces.Info;
using LMS.Services.Interfaces.CommonService;
using System.Linq.Expressions;

namespace LMS.Services.Impl.CommonService;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _userRepository.GetByIdAsync(id, asNoTracking: true, ct);

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => _userRepository.GetByUsernameAsync(username, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _userRepository.GetByEmailAsync(email, ct);

    public async Task<PagedResult<User>> ListAsync(
        Expression<Func<User, bool>>? predicate = null,
        Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null,
        int pageIndex = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 50;

        var skip = (pageIndex - 1) * pageSize;

        var items = await _userRepository.ListAsync(
            predicate: predicate,
            orderBy: orderBy,
            skip: skip,
            take: pageSize,
            asNoTracking: true,
            includes: null,
            ct: ct);

        var total = await _userRepository.CountAsync(predicate, ct);

        return new PagedResult<User>(items, total, pageIndex, pageSize);
    }

    public Task<IReadOnlyList<User>> GetByRoleAsync(string roleDesc, CancellationToken ct = default)
        => _userRepository.GetByRoleAsync(roleDesc, ct);

    public Task<bool> IsUsernameExistsAsync(string username, Guid? excludeUserId = null, CancellationToken ct = default)
        => _userRepository.IsUsernameExistsAsync(username, excludeUserId, ct);

    public Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken ct = default)
        => _userRepository.IsEmailExistsAsync(email, excludeUserId, ct);

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        user.UserId = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;
        return await _userRepository.AddAsync(user, saveNow: true, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, saveNow: true, ct);
    }

    public Task DeleteAsync(User user, CancellationToken ct = default)
        => _userRepository.DeleteAsync(user, saveNow: true, ct);

    public async Task DeleteByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByIdAsync(id, asNoTracking: false, ct);
        if (user != null)
        {
            await _userRepository.DeleteAsync(user, saveNow: true, ct);
        }
    }
}

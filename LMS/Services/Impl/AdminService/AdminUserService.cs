using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Services.Interfaces.AdminService;
using LMS.Services.Interfaces.CommonService;
using System.Linq.Expressions;

namespace LMS.Services.Impl.AdminService;

public class AdminUserService : IAdminUserService
{
    private readonly IUserService _userService;

    public AdminUserService(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<PagedResult<User>> GetUsersAsync(
        string? searchTerm = null,
        string? roleFilter = null,
        bool? isActiveFilter = null,
        int pageIndex = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        // Build predicate properly for EF Core - capture variables outside the expression
        Expression<Func<User, bool>>? predicate = null;
        
        var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
        var hasRole = !string.IsNullOrWhiteSpace(roleFilter);
        var hasActiveFilter = isActiveFilter.HasValue;

        if (hasSearch || hasRole || hasActiveFilter)
        {
            // Capture values outside the expression to avoid closure issues
            var searchLower = hasSearch ? searchTerm!.ToLower() : null;
            var role = roleFilter;
            var isActive = isActiveFilter;

            predicate = u =>
                (!hasSearch ||
                 u.Username.ToLower().Contains(searchLower!) ||
                 u.Email.ToLower().Contains(searchLower!) ||
                 (u.FullName != null && u.FullName.ToLower().Contains(searchLower!))) &&
                (!hasRole || u.RoleDesc == role) &&
                (!hasActiveFilter || u.IsActive == isActive!.Value);
        }

        Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = q => q.OrderByDescending(u => u.CreatedAt);

        return await _userService.ListAsync(
            predicate: predicate,
            orderBy: orderBy,
            pageIndex: pageIndex,
            pageSize: pageSize,
            ct: ct);
    }

    public Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct = default)
        => _userService.GetByIdAsync(id, ct);

    public Task<User> CreateUserAsync(User user, CancellationToken ct = default)
        => _userService.CreateAsync(user, ct);

    public Task UpdateUserAsync(User user, CancellationToken ct = default)
        => _userService.UpdateAsync(user, ct);

    public Task DeleteUserAsync(Guid id, CancellationToken ct = default)
        => _userService.DeleteByIdAsync(id, ct);

    public async Task ToggleUserActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _userService.GetByIdAsync(id, ct);
        if (user == null)
            throw new InvalidOperationException($"User with ID {id} not found.");

        user.IsActive = !user.IsActive;
        await _userService.UpdateAsync(user, ct);
    }
}

using LMS.Models.Entities;
using LMS.Models.ViewModels;
using System.Linq.Expressions;

namespace LMS.Services.Interfaces.AdminService;

public interface IAdminUserService
{
    Task<PagedResult<User>> GetUsersAsync(
        string? searchTerm = null,
        string? roleFilter = null,
        bool? isActiveFilter = null,
        int pageIndex = 1,
        int pageSize = 50,
        CancellationToken ct = default);
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<User> CreateUserAsync(User user, CancellationToken ct = default);
    Task UpdateUserAsync(User user, CancellationToken ct = default);
    Task DeleteUserAsync(Guid id, CancellationToken ct = default);
    Task ToggleUserActiveStatusAsync(Guid id, CancellationToken ct = default);
}

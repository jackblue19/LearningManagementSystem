using System.Linq.Expressions;
using LMS.Models.ViewModels;

namespace LMS.Services.Interfaces;

public interface ICrudService<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(
        TKey id,
        bool asNoTracking = true,
        IEnumerable<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    Task<PagedResult<T>> ListAsync(
                            Expression<Func<T, bool>>? predicate = null,
                            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                            int pageIndex = 1,
                            int pageSize = 50,
                            bool asNoTracking = true,
                            IEnumerable<Expression<Func<T, object>>>? includes = null,
                            CancellationToken ct = default);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate,
                            CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                            CancellationToken ct = default);

    Task<T> CreateAsync(T entity, bool saveNow = false,
                            CancellationToken ct = default);
    Task CreateRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                            CancellationToken ct = default);

    Task UpdateAsync(T entity, bool saveNow = false,
                            CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                            CancellationToken ct = default);

    Task DeleteAsync(T entity, bool saveNow = false,
                            CancellationToken ct = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                            CancellationToken ct = default);
    Task DeleteByIdAsync(TKey id, bool saveNow = false,
                            CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}

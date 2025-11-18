using System.Linq.Expressions;

namespace LMS.Repositories;

public interface IGenericRepository<T, TKey> where T : class
{
    Task<T?> GetByIdAsync(
        TKey id,
        bool asNoTracking = true,
        CancellationToken ct = default);

    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool asNoTracking = true,
        IEnumerable<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = true,
        IEnumerable<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);

    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    Task<T> AddAsync(T entity, bool saveNow = true, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, bool saveNow = true, CancellationToken ct = default);
    Task UpdateAsync(T entity, bool saveNow = true, CancellationToken ct = default);
    Task UpdateRangeAsync(IEnumerable<T> entities, bool saveNow = true, CancellationToken ct = default);
    Task DeleteAsync(T entity, bool saveNow = true, CancellationToken ct = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, bool saveNow = true, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}

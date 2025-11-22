using System.Linq.Expressions;
using LMS.Models.Entities;

namespace LMS.Repositories.Abstraction;
public sealed record PagedResult<T>(IReadOnlyList<T> Items,
                                    int TotalCount,
                                    int PageIndex,
                                    int PageSize)
{
    public static implicit operator PagedResult<T>(Models.ViewModels.PagedResult<Class> v)
    {
        throw new NotImplementedException();
    }
}

public interface IRepository<T, TKey>
where T : class
where TKey : notnull
{
    // Read
    Task<T?> GetByIdAsync(
                TKey id,
                bool asNoTracking = true,
                IEnumerable<Expression<Func<T, object>>>? includes = null,
                bool splitQuery = false,
                CancellationToken cancellationToken = default);

    Task<T?> FirstOrDefaultAsync(
                Expression<Func<T, bool>> predicate,
                bool asNoTracking = true,
                IEnumerable<Expression<Func<T, object>>>? includes = null,
                bool splitQuery = false,
                CancellationToken cancellationToken = default);


    Task<IReadOnlyList<T>> ListAsync(
                            Expression<Func<T, bool>>? predicate = null,
                            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                            int? skip = null,
                            int? take = null,
                            bool asNoTracking = true,
                            IEnumerable<Expression<Func<T, object>>>? includes = null,
                            bool splitQuery = false,
                            CancellationToken cancellationToken = default);


    Task<PagedResult<T>> PagedListAsync(
                            Expression<Func<T, bool>>? predicate = null,
                            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                            int pageIndex = 1,
                            int pageSize = 20,
                            bool asNoTracking = true,
                            IEnumerable<Expression<Func<T, object>>>? includes = null,
                            bool splitQuery = false,
                            CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate,
                            CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                            CancellationToken cancellationToken = default);

    // Create
    Task<T> AddAsync(T entity, bool saveNow = false,
                        CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                        CancellationToken cancellationToken = default);


    // Update
    Task UpdateAsync(T entity, bool saveNow = false,
                        CancellationToken cancellationToken = default);
    Task UpdatePartialAsync(T entity, bool saveNow = false,
                            CancellationToken cancellationToken = default,
                            params Expression<Func<T, object>>[] modifiedProperties);
    Task UpdateRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                            CancellationToken cancellationToken = default);

    // Delete
    Task DeleteAsync(T entity, bool saveNow = false,
                        CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                            CancellationToken cancellationToken = default);
    Task DeleteByIdAsync(TKey id, bool saveNow = false,
                            CancellationToken cancellationToken = default);

    // UoW bridge
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

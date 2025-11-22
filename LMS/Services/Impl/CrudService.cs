using LMS.Models.ViewModels;
using LMS.Repositories;
using LMS.Services.Interfaces;
using System.Linq.Expressions;

namespace LMS.Services.Impl;

public class CrudService<T, TKey> : ICrudService<T, TKey> where T : class
{
    private readonly IGenericRepository<T, TKey> _repo;

    public CrudService(IGenericRepository<T, TKey> repo)
    {
        _repo = repo;
    }

    public Task<T?> GetByIdAsync(
            TKey id,
            bool asNoTracking = true,
            IEnumerable<Expression<Func<T, object>>>? includes = null,
            CancellationToken ct = default)
            => _repo.GetByIdAsync(id, asNoTracking, ct);

    public async Task<PagedResult<T>> ListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int pageIndex = 1,
            int pageSize = 50,
            bool asNoTracking = true,
            IEnumerable<Expression<Func<T, object>>>? includes = null,
            CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 50;

        var skip = (pageIndex - 1) * pageSize;

        var items = await _repo.ListAsync(
            predicate: predicate,
            orderBy: orderBy,
            skip: skip,
            take: pageSize,
            asNoTracking: asNoTracking,
            includes: includes,
            ct: ct);

        var total = await _repo.CountAsync(predicate, ct);

        return new PagedResult<T>(items, total, pageIndex, pageSize);
    }

    public virtual Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate,
                                    CancellationToken ct = default)
        => _repo.ExistsAsync(predicate, ct);

    public virtual Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                                    CancellationToken ct = default)
        => _repo.CountAsync(predicate, ct);

    public virtual Task<T> CreateAsync(T entity, bool saveNow = false,
                                CancellationToken ct = default)
        => _repo.AddAsync(entity, saveNow, ct);

    public virtual Task CreateRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                                    CancellationToken ct = default)
        => _repo.AddRangeAsync(entities, saveNow, ct);

    public virtual Task UpdateAsync(T entity, bool saveNow = false,
                                CancellationToken ct = default)
        => _repo.UpdateAsync(entity, saveNow, ct);

    public virtual Task UpdateRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                                    CancellationToken ct = default)
        => _repo.UpdateRangeAsync(entities, saveNow, ct);

    public virtual Task DeleteAsync(T entity, bool saveNow = false,
                                CancellationToken ct = default)
        => _repo.DeleteAsync(entity, saveNow, ct);

    public virtual Task DeleteRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                                    CancellationToken ct = default)
        => _repo.DeleteRangeAsync(entities, saveNow, ct);

    public virtual async Task DeleteByIdAsync(TKey id, bool saveNow = false,
                                        CancellationToken ct = default)
    {
        var found = await _repo.GetByIdAsync(id, asNoTracking: false, ct);
        if (found is null) return;
        await _repo.DeleteAsync(found, saveNow, ct);
    }

    public virtual Task SaveChangesAsync(CancellationToken ct = default)
        => _repo.SaveChangesAsync(ct);
}

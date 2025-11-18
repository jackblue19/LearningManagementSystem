using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using LMS.Data;

namespace LMS.Repositories;

public class GenericRepository<T, TKey> : IGenericRepository<T, TKey>
    where T : class
{
    private readonly CenterDbContext _db;
    private readonly DbSet<T> _set;

    public GenericRepository(CenterDbContext db)
    {
        _db = db;
        _set = _db.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(
        TKey id,
        bool asNoTracking = true,
        CancellationToken ct = default)
    {
        var entity = await _set.FindAsync(new object?[] { id! }, ct);
        if (entity is null) return null;
        if (asNoTracking) _db.Entry(entity).State = EntityState.Detached;
        return entity;
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        bool asNoTracking = true,
        IEnumerable<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default)
    {
        IQueryable<T> query = _set;
        if (asNoTracking) query = query.AsNoTracking();
        query = ApplyIncludes(query, includes);
        return await query.FirstOrDefaultAsync(predicate, ct);
    }

    public virtual async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        int? skip = null,
        int? take = null,
        bool asNoTracking = true,
        IEnumerable<Expression<Func<T, object>>>? includes = null,
        CancellationToken ct = default)
    {
        IQueryable<T> query = _set;
        if (asNoTracking) query = query.AsNoTracking();
        query = ApplyIncludes(query, includes);
        if (predicate is not null) query = query.Where(predicate);
        if (orderBy is not null) query = orderBy(query);
        if (skip is > 0) query = query.Skip(skip.Value);
        if (take is > 0) query = query.Take(take.Value);
        return await query.ToListAsync(ct);
    }

    public virtual Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default)
        => _set.AnyAsync(predicate, ct);

    public virtual Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default)
        => predicate is null ? _set.CountAsync(ct) : _set.CountAsync(predicate, ct);

    public virtual async Task<T> AddAsync(T entity, bool saveNow = true, CancellationToken ct = default)
    {
        await _set.AddAsync(entity, ct);
        if (saveNow) await _db.SaveChangesAsync(ct);
        return entity;
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, bool saveNow = true, CancellationToken ct = default)
    {
        await _set.AddRangeAsync(entities, ct);
        if (saveNow) await _db.SaveChangesAsync(ct);
    }

    public virtual async Task UpdateAsync(T entity, bool saveNow = true, CancellationToken ct = default)
    {
        _set.Update(entity);
        if (saveNow) await _db.SaveChangesAsync(ct);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<T> entities, bool saveNow = true, CancellationToken ct = default)
    {
        _set.UpdateRange(entities);
        if (saveNow) await _db.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteAsync(T entity, bool saveNow = true, CancellationToken ct = default)
    {
        _set.Remove(entity);
        if (saveNow) await _db.SaveChangesAsync(ct);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<T> entities, bool saveNow = true, CancellationToken ct = default)
    {
        _set.RemoveRange(entities);
        if (saveNow) await _db.SaveChangesAsync(ct);
    }

    public virtual Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    private static IQueryable<T> ApplyIncludes(
        IQueryable<T> query,
        IEnumerable<Expression<Func<T, object>>>? includes)
    {
        if (includes is null) return query;
        foreach (var include in includes) query = query.Include(include);
        return query;
    }
}

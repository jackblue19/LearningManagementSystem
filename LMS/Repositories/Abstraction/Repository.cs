using LMS.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LMS.Repositories.Abstraction;

public class Repository<T, TKey> : IRepository<T, TKey>
    where T : class
    where TKey : notnull
{
    private readonly CenterDbContext _ctx;
    private readonly DbSet<T> _set;

    public Repository(CenterDbContext ctx)
    {
        _ctx = ctx;
        _set = _ctx.Set<T>();
    }

    // ===== Read =====
    public async Task<T?> GetByIdAsync(
                            TKey id,
                            bool asNoTracking = true,
                            IEnumerable<Expression<Func<T, object>>>? includes = null,
                            bool splitQuery = false,
                            CancellationToken cancellationToken = default)
    {
        if (includes == null || !includes.Any())
        {
            var found = await _set.FindAsync(new object?[] { id }, cancellationToken);
            if (found is null) return null;
            if (asNoTracking)
            {
                _ctx.Entry(found).State = EntityState.Detached;
            }
            return found;
        }

        var keyName = GetSinglePrimaryKeyNameOrNull();
        if (keyName is null)
        {
            throw new NotSupportedException(
                "GetByIdAsync(includes) chỉ hỗ trợ entity " +
                "có khóa đơn (single-column PK).");
        }

        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Call(typeof(EF), nameof(EF.Property),
                new[] { typeof(TKey) }, parameter, Expression.Constant(keyName));
        var equals = Expression.Equal(property, Expression.Constant(id));
        var lambda = Expression.Lambda<Func<T, bool>>(equals, parameter);

        var query = BuildQuery(lambda, includes, asNoTracking, splitQuery);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<T?> FirstOrDefaultAsync(
                            Expression<Func<T, bool>> predicate,
                            bool asNoTracking = true,
                            IEnumerable<Expression<Func<T, object>>>? includes = null,
                            bool splitQuery = false,
                            CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(predicate, includes, asNoTracking, splitQuery);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<T>> ListAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            int? skip = null,
            int? take = null,
            bool asNoTracking = true,
            IEnumerable<Expression<Func<T, object>>>? includes = null,
            bool splitQuery = false,
            CancellationToken cancellationToken = default)
    {
        var query = BuildQuery(predicate, includes, asNoTracking, splitQuery);
        query = ApplySortingForPagingFallback(query, orderBy, skip, take);
        if (skip.HasValue) query = query.Skip(skip.Value);
        if (take.HasValue) query = query.Take(take.Value);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<T>> PagedListAsync(
                    Expression<Func<T, bool>>? predicate = null,
                    Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                    int pageIndex = 1,
                    int pageSize = 20,
                    bool asNoTracking = true,
                    IEnumerable<Expression<Func<T, object>>>? includes = null,
                    bool splitQuery = false,
                    CancellationToken cancellationToken = default)
    {
        if (pageIndex <= 0) pageIndex = 1;
        if (pageSize <= 0) pageSize = 20;

        var baseQuery = BuildQuery(predicate, includes, asNoTracking, splitQuery);
        var total = await baseQuery.CountAsync(cancellationToken);

        var query = ApplySortingForPagingFallback(baseQuery, orderBy,
                                                    skip: null, take: null);
        var items = await query
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, total, pageIndex, pageSize);
    }

    public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate,
                                    CancellationToken cancellationToken = default)
    => _set.AnyAsync(predicate, cancellationToken);


    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
                                    CancellationToken cancellationToken = default)
    => predicate is null ? _set.CountAsync(cancellationToken) : _set.CountAsync(predicate,
                                                                        cancellationToken);

    // ===== Create =====
    public async Task<T> AddAsync(T entity, bool saveNow = false,
                                    CancellationToken cancellationToken = default)
    {
        await _set.AddAsync(entity, cancellationToken);
        if (saveNow) await _ctx.SaveChangesAsync(cancellationToken);
        return entity;
    }


    public async Task AddRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                                    CancellationToken cancellationToken = default)
    {
        await _set.AddRangeAsync(entities, cancellationToken);
        if (saveNow) await _ctx.SaveChangesAsync(cancellationToken);
    }

    // ===== Update =====
    public async Task UpdateAsync(T entity, bool saveNow = false,
                                    CancellationToken cancellationToken = default)
    {
        var entry = _ctx.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _set.Attach(entity);
            entry = _ctx.Entry(entity);
        }
        entry.State = EntityState.Modified;
        if (saveNow) await _ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePartialAsync(T entity, bool saveNow = false,
                                    CancellationToken cancellationToken = default,
                                    params Expression<Func<T, object>>[] modifiedProperties)
    {
        var entry = _ctx.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            _set.Attach(entity);
            entry = _ctx.Entry(entity);
        }

        entry.State = EntityState.Unchanged;
        foreach (var expr in modifiedProperties)
        {
            var name = GetPropertyName(expr);
            entry.Property(name).IsModified = true;
        }

        if (saveNow) await _ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateRangeAsync(IEnumerable<T> entities,
                                        bool saveNow = false,
                                        CancellationToken cancellationToken = default)
    {
        foreach (var e in entities)
        {
            var entry = _ctx.Entry(e);
            if (entry.State == EntityState.Detached)
            {
                _set.Attach(e);
                entry = _ctx.Entry(e);
            }
            entry.State = EntityState.Modified;
        }
        if (saveNow) await _ctx.SaveChangesAsync(cancellationToken);
    }

    // ===== Delete =====
    public async Task DeleteAsync(T entity, bool saveNow = false,
                                    CancellationToken cancellationToken = default)
    {
        _set.Remove(entity);
        if (saveNow) await _ctx.SaveChangesAsync(cancellationToken);
    }
    public async Task DeleteRangeAsync(IEnumerable<T> entities, bool saveNow = false,
                                        CancellationToken cancellationToken = default)
    {
        _set.RemoveRange(entities);
        if (saveNow) await _ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByIdAsync(TKey id, bool saveNow = false,
                                        CancellationToken cancellationToken = default)
    {
        var keyName = GetSinglePrimaryKeyNameOrNull();
        if (keyName is null)
        {
            // Fall back to load-then-delete if không xác định được PK đơn.
            var entity = await GetByIdAsync(id, asNoTracking: false, includes: null,
                                            splitQuery: false, cancellationToken);
            if (entity is null) return;
            _set.Remove(entity);
        }
        else
        {
            var stub = Activator.CreateInstance<T>();
            _ctx.Entry(stub!).Property(keyName).CurrentValue = id;
            _ctx.Entry(stub!).State = EntityState.Deleted;
        }

        if (saveNow) await _ctx.SaveChangesAsync(cancellationToken);
    }

    // ===== UoW bridge =====
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    => _ctx.SaveChangesAsync(cancellationToken);


    // ===== Helpers =====
    private IQueryable<T> BuildQuery(
                            Expression<Func<T, bool>>? predicate,
                            IEnumerable<Expression<Func<T, object>>>? includes,
                            bool asNoTracking,
                            bool splitQuery)
    {
        IQueryable<T> query = _set;
        if (asNoTracking) query = query.AsNoTracking();
        if (splitQuery) query = query.AsSplitQuery();
        if (predicate is not null) query = query.Where(predicate);
        if (includes is not null)
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }
        return query;
    }

    private IQueryable<T> ApplySortingForPagingFallback(
                            IQueryable<T> query,
                            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy,
                            int? skip,
                            int? take)
    {
        if (orderBy is not null) return orderBy(query);
        if (!skip.HasValue && !take.HasValue) return query;
        var keyName = GetSinglePrimaryKeyNameOrNull();
        if (keyName is null) return query; // không ép sắp xếp khi không tìm được PK đơn
        return query.OrderBy(e => EF.Property<object>(e, keyName));
    }

    private string? GetSinglePrimaryKeyNameOrNull()
    {
        var entityType = _ctx.Model.FindEntityType(typeof(T));
        var pk = entityType?.FindPrimaryKey();
        if (pk == null || pk.Properties.Count != 1) return null;
        return pk.Properties[0].Name;
    }

    private static string GetPropertyName(Expression<Func<T, object>> expression)
    {
        if (expression.Body is UnaryExpression u && u.Operand is MemberExpression m1)
            return m1.Member.Name;
        if (expression.Body is MemberExpression m2)
            return m2.Member.Name;
        throw new ArgumentException("Biểu thức không trỏ tới property.");
    }
}

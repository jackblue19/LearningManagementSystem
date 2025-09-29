using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class AuditLog
{
    [Key]
    public long LogId { get; set; }

    public Guid UserId { get; set; }

    [StringLength(40)]
    public string? ActionType { get; set; }

    [StringLength(100)]
    public string? EntityName { get; set; }

    [StringLength(100)]
    public string? RecordId { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public string? NewData { get; set; }

    public string? OldData { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AuditLogs")]
    public virtual User User { get; set; } = null!;
}



#region Enterprise style

/*  Base Entity với Audit + Soft Delete
public abstract class AuditableEntity<TKey>
{
    public TKey Id { get; set; } = default!;

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Soft delete flag
    public bool IsDeleted { get; set; } = false;
}
*/

/*  Order entity
public class Order : AuditableEntity<Guid>
{
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
}
*/


/*  Global Query Filter (EF Core) trong "ApplicationDbContext"
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        if (typeof(AuditableEntity<Guid>).IsAssignableFrom(entityType.ClrType))
        {
            modelBuilder.Entity(entityType.ClrType)
                        .HasQueryFilter(GetIsDeletedRestriction(entityType.ClrType));
        }
    }
}

private static LambdaExpression GetIsDeletedRestriction(Type type)
{
    var param = Expression.Parameter(type, "e");
    var prop = Expression.Property(param, nameof(AuditableEntity<Guid>.IsDeleted));
    var condition = Expression.Equal(prop, Expression.Constant(false));
    return Expression.Lambda(condition, param);
}
*/


/*  RepositoryBase với Audit + Soft Delete
public class RepositoryBase<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : AuditableEntity<TKey>
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;
    private readonly ICurrentUserService _currentUser;

    public RepositoryBase(ApplicationDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
        _currentUser = currentUser;
    }

    public async Task<TEntity?> GetByIdAsync(TKey id)
        => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<TEntity>> GetAllAsync()
        => await _dbSet.ToListAsync();

    public async Task AddAsync(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = _currentUser.UserId;
        await _dbSet.AddAsync(entity);
    }

    public Task UpdateAsync(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = _currentUser.UserId;
        _dbSet.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = _currentUser.UserId;
        _dbSet.Update(entity); // soft delete
        return Task.CompletedTask;
    }
}
*/

/*  Audit Logging (AuditLog Entity) <như gốc class này>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityName { get; set; } = "";
    public string Action { get; set; } = ""; // Create, Update, Delete
    public string? UserId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Changes { get; set; } // JSON diff
}
*/

/*  Trong UnitOfWork.CommitAsync() ta capture log
public async Task<int> CommitAsync()
{
    var auditEntries = new List<AuditLog>();

    foreach (var entry in _dbContext.ChangeTracker.Entries<AuditableEntity<Guid>>())
    {
        if (entry.State == EntityState.Added ||
            entry.State == EntityState.Modified ||
            entry.State == EntityState.Deleted)
        {
            var audit = new AuditLog
            {
                EntityName = entry.Entity.GetType().Name,
                Action = entry.State.ToString(),
                UserId = entry.Entity.UpdatedBy ?? entry.Entity.CreatedBy,
                Timestamp = DateTime.UtcNow,
                Changes = JsonSerializer.Serialize(entry.CurrentValues.ToObject())
            };
            auditEntries.Add(audit);
        }
    }

    if (auditEntries.Any())
        await _dbContext.Set<AuditLog>().AddRangeAsync(auditEntries);

    return await _dbContext.SaveChangesAsync();
}
*/

/* Full UoW impl
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOrderRepository Orders { get; }
    Task<int> CommitAsync();
    Task RollbackAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext;
    public IUserRepository Users { get; }
    public IOrderRepository Orders { get; }

    public UnitOfWork(ApplicationDbContext dbContext,
                      IUserRepository userRepo,
                      IOrderRepository orderRepo)
    {
        _dbContext = dbContext;
        Users = userRepo;
        Orders = orderRepo;
    }

    public async Task<int> CommitAsync()
        => await _dbContext.SaveChangesAsync();

    public Task RollbackAsync()
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries())
            entry.State = EntityState.Unchanged;
        return Task.CompletedTask;
    }

    public void Dispose() => _dbContext.Dispose();
}
*/


/*  Service sử dụng UoW + Audit
public class OrderAppService : AppServiceBase, IOrderAppService
{
    private readonly IUnitOfWork _uow;

    public OrderAppService(IUnitOfWork uow, IMapper mapper, ILogger<OrderAppService> logger)
        : base(uow, mapper, logger)
    {
        _uow = uow;
    }

    public async Task<Guid> CreateOrderAsync(CreateOrderDto dto)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var order = _mapper.Map<Order>(dto);

            await _uow.Orders.AddAsync(order);
            await _uow.CommitAsync(); // sẽ tự log audit

            return order.Id;
        });
    }

    public async Task DeleteOrderAsync(Guid orderId)
    {
        await ExecuteWithTransactionAsync(async () =>
        {
            var order = await _uow.Orders.GetByIdAsync(orderId);
            if (order == null) throw new DomainException("Order not found.");

            await _uow.Orders.DeleteAsync(order);
            await _uow.CommitAsync();
        });
    }
}
*/

#endregion

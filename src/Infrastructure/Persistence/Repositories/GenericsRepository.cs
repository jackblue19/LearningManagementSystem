using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence.Repositories;
internal class GenericsRepository
{
}

#region Repo + UoW


/*  RepositoryBase
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MySolution.Infrastructure.Persistence.Repositories
{
    public class RepositoryBase<TEntity, TKey> where TEntity : class
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;

        public RepositoryBase(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbSet = _dbContext.Set<TEntity>();
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        public virtual Task DeleteAsync(TEntity entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
    }
}
*/

/*  Use of RepoBase
public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
{
    public UserRepository(ApplicationDbContext dbContext) : base(dbContext) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.Value == email);
    }
}
*/


/*  RepositoryBase + UoW
public interface IRepository<TEntity, TKey> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TKey id);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task AddAsync(TEntity entity);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
}
*/

/*  Trên là "IRepository<T>", còn đây là "RepositoryBase<T>"
public class RepositoryBase<TEntity, TKey> : IRepository<TEntity, TKey>
    where TEntity : class
{
    protected readonly ApplicationDbContext _dbContext;
    protected readonly DbSet<TEntity> _dbSet;

    public RepositoryBase(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = dbContext.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(TKey id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<TEntity>> GetAllAsync() => await _dbSet.ToListAsync();

    public async Task AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);

    public Task UpdateAsync(TEntity entity) { _dbSet.Update(entity); return Task.CompletedTask; }

    public Task DeleteAsync(TEntity entity) { _dbSet.Remove(entity); return Task.CompletedTask; }
}
*/


/* IUnitOfWork
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOrderRepository Orders { get; }

    Task<int> CommitAsync();
    Task RollbackAsync();
}
*/

/*  Impl UOW
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

    public async Task<int> CommitAsync() => await _dbContext.SaveChangesAsync();

    public Task RollbackAsync()
    {
        foreach (var entry in _dbContext.ChangeTracker.Entries())
        {
            entry.State = EntityState.Unchanged;
        }
        return Task.CompletedTask;
    }

    public void Dispose() => _dbContext.Dispose();
}
*/

#endregion


/*  Unrelated
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

            // có thể chèn logic khác (audit log, gửi email)
            await _uow.Users.GetByIdAsync(dto.UserId);

            await _uow.CommitAsync();

            return order.Id;
        });
    }
}
*/

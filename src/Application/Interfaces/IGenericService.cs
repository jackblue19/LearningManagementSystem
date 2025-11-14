using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces;
internal interface IGenericService
{
}

public interface ICrudService<TDto, in TKey>
{
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<TDto?> GetByIdAsync(TKey id);
    Task<TDto> CreateAsync(TDto dto);
    Task<TDto> UpdateAsync(TKey id, TDto dto);
    Task<bool> DeleteAsync(TKey id);
}

public interface IQueryService<TDto, TFilter>
{
    Task<IEnumerable<TDto>> GetAsync(TFilter filter);
    Task<TDto?> GetByIdAsync(Guid id);
}

public interface ITransactionalService
{
    Task<int> CommitAsync();
}

/*
public interface IUserService : ICrudService<UserDto, Guid>, ITransactionalService { }

public interface IUserService : ICrudService<UserDto, Guid>
{
    Task<UserDto?> GetByEmailAsync(string email);
    Task<bool> ResetPasswordAsync(Guid id);
}

public interface IStudentExamResultService : IQueryService<ExamResultDto, ExamFilter>
{
    Task<IEnumerable<ExamResultDto>> GetByStudentIdAsync(Guid studentId);
}

public interface IPaymentService : ICrudService<PaymentDto, Guid>, ITransactionalService
{
    Task<PaymentDto> PayTuitionAsync(Guid studentId, decimal amount);
}

public class UserService : CrudService<User, UserDto, Guid>, IUserService
{
    public UserService(IGenericRepository<User> repo, IMapper mapper)
        : base(repo, mapper) { }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _repository.Query().FirstOrDefaultAsync(u => u.Email == email);
        return _mapper.Map<UserDto?>(user);
    }
}


public abstract class CrudService<TEntity, TDto, TKey> : ICrudService<TDto, TKey>
{
    protected readonly IGenericRepository<TEntity> _repository;
    protected readonly IMapper _mapper;

    public CrudService(IGenericRepository<TEntity> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public virtual async Task<IEnumerable<TDto>> GetAllAsync()
        => _mapper.Map<IEnumerable<TDto>>(await _repository.GetAllAsync());

    public virtual async Task<TDto?> GetByIdAsync(TKey id)
        => _mapper.Map<TDto?>(await _repository.GetByIdAsync(id));

    public virtual async Task<TDto> CreateAsync(TDto dto)
    {
        var entity = _mapper.Map<TEntity>(dto);
        await _repository.AddAsync(entity);
        await _repository.CommitAsync();
        return _mapper.Map<TDto>(entity);
    }

    // Update, Delete tương tự...
}


 */

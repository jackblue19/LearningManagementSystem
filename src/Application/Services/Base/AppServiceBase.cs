using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Application.Services.Base;
internal class AppServiceBase
{
}


#region Sample Code

/*  Full impl
    public abstract class AppServiceBase
    {
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IMapper _mapper;
        protected readonly ILogger _logger;

        protected AppServiceBase(IUnitOfWork unitOfWork, IMapper mapper, ILogger logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Wrap một use-case vào UnitOfWork transaction.
        /// </summary>
        protected async Task<TResult> ExecuteWithTransactionAsync<TResult>(Func<Task<TResult>> action)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                var result = await action();

                await _unitOfWork.CommitAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}", this.GetType().Name);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// Wrap void action.
        /// </summary>
        protected async Task ExecuteWithTransactionAsync(Func<Task> action)
        {
            try
            {
                _unitOfWork.BeginTransaction();

                await action();

                await _unitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {Service}", this.GetType().Name);
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
*/


/*  CRUD → try/catch + transaction + logging
public class OrderAppService : AppServiceBase, IOrderAppService
{
    private readonly IOrderRepository _orderRepository;

    public OrderAppService(IOrderRepository orderRepository, IUnitOfWork uow, IMapper mapper, ILogger<OrderAppService> logger)
        : base(uow, mapper, logger)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Guid> CreateOrderAsync(CreateOrderDto dto)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var order = _mapper.Map<Order>(dto);

            await _orderRepository.AddAsync(order);

            return order.Id;
        });
    }
}
*/


/*  Validation
public async Task UpdateOrderAsync(UpdateOrderDto dto)
{
    await ExecuteWithTransactionAsync(async () =>
    {
        var order = await _orderRepository.GetByIdAsync(dto.Id);
        if (order == null)
            throw new DomainException("Order not found");

        order.UpdateDetails(dto.NewAddress, dto.Status);

        await _orderRepository.UpdateAsync(order);
    });
}
*/


/*  External Service
public class UserAppService : AppServiceBase, IUserAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailSender _emailSender;

    public UserAppService(IUserRepository userRepository, IEmailSender emailSender, IUnitOfWork uow, IMapper mapper, ILogger<UserAppService> logger)
        : base(uow, mapper, logger)
    {
        _userRepository = userRepository;
        _emailSender = emailSender;
    }

    public async Task<Guid> RegisterUserAsync(RegisterUserDto dto)
    {
        return await ExecuteWithTransactionAsync(async () =>
        {
            var user = new User(dto.Name, Email.Create(dto.Email));
            await _userRepository.AddAsync(user);

            await _emailSender.SendAsync(user.Email.Value, "Welcome!", "Thanks for joining us!");

            return user.Id;
        });
    }
}
*/


/*  Read-Only Query
protected async Task<TResult> ExecuteReadOnlyAsync<TResult>(Func<Task<TResult>> action)
{
    try
    {
        return await action();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "ReadOnly error in {Service}", this.GetType().Name);
        throw;
    }
}
*/

#endregion

using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Finance;

public interface IPaymentRepository
    : IGenericRepository<Payment, Guid>
{
}

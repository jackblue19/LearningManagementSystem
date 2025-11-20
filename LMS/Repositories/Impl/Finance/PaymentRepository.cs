using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Finance;

namespace LMS.Repositories.Impl.Finance;

public class PaymentRepository : GenericRepository<Payment, Guid>, IPaymentRepository
{
    public PaymentRepository(CenterDbContext db) : base(db)
    {
    }
}

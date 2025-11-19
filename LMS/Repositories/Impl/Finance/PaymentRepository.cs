using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;
using System;

namespace LMS.Repositories.Impl.Finance;
public class PaymentRepository 
    : GenericRepository<Payment, Guid>, IPaymentRepository
{
    public PaymentRepository(CenterDbContext db) : base(db)
    {
    }
}

using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.Bank;
using LMS.Repositories;
using LMS.Services.Interfaces.StudentService;
using System.Linq.Expressions;

namespace LMS.Services.Impl.StudentService;

public class PaymentService : IPaymentService
{
    private readonly IGenericRepository<Payment, Guid> _payRepo;
    private readonly IGenericRepository<ClassRegistration, long> _regRepo;
    private readonly IGenericRepository<Class, Guid> _classRepo;

    public PaymentService(
        IGenericRepository<Payment, Guid> payRepo,
        IGenericRepository<ClassRegistration, long> regRepo,
        IGenericRepository<Class, Guid> classRepo)
    {
        _payRepo = payRepo;
        _regRepo = regRepo;
        _classRepo = classRepo;
    }
    /// <summary>
    /// 1. tạm thời thì fake auto thành công :v
    /// 2. đó trưa thi xong tích hợp thêm vnpay hoặc j đó khác sau :">
    /// 3. đó sẽ refactor code lại
    /// </summary>
    /// <param name="studentId"></param>
    /// <param name="classId"></param>
    /// <param name="paymentMethod"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<PaymentCheckoutVm> CreateRegistrationPaymentAsync(
        Guid studentId, Guid classId, string paymentMethod, CancellationToken ct = default)
    {
        var reg = await _regRepo.FirstOrDefaultAsync(
                    r => r.StudentId == studentId && r.ClassId == classId
                                                  && r.RegistrationStatus == "approved",
                            asNoTracking: false,
                            includes: new Expression<Func<ClassRegistration, object>>[]
                                        { r => r.Class! },
                            ct: ct);

        if (reg is null || reg.Class is null)
            throw new InvalidOperationException("Registration not found or not approved.");

        var cls = reg.Class;
        var amount = (cls.UnitPrice ?? 0m) * (cls.TotalSessions ?? 0);

        var payment = new Payment
        {
            StudentId = studentId,
            ClassId = classId,
            RegistrationId = reg.RegistrationId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            PaymentStatus = "paid",
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow
        };

        await _payRepo.AddAsync(payment, saveNow: false, ct);
        await _payRepo.SaveChangesAsync(ct);

        return new PaymentCheckoutVm(
                    payment.PaymentId, payment.RegistrationId, classId, amount,
                    payment.PaymentMethod, payment.PaymentStatus,
                    payment.CreatedAt, payment.PaidAt, cls.ClassName);
    }

    public async Task<PagedResult<PaymentHistoryVm>> ListMyPaymentsAsync(
        Guid studentId, DateOnly? from = null, DateOnly? to = null, string? status = null,
        int pageIndex = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;
        var skip = (pageIndex - 1) * pageSize;

        var pays = await _payRepo.ListAsync(
            p => p.StudentId == studentId
                 && (!from.HasValue || p.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue))
                 && (!to.HasValue || p.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue))
                 && (string.IsNullOrEmpty(status) || p.PaymentStatus == status),
            orderBy: q => q.OrderByDescending(p => p.CreatedAt),
            skip: skip, take: pageSize,
            asNoTracking: true,
            includes: new Expression<Func<Payment, object>>[]
                        { p => p.Class! },
            ct: ct);

        var total = await _payRepo.CountAsync(
            p => p.StudentId == studentId
                 && (!from.HasValue || p.CreatedAt >= from.Value.ToDateTime(TimeOnly.MinValue))
                 && (!to.HasValue || p.CreatedAt <= to.Value.ToDateTime(TimeOnly.MaxValue))
                 && (string.IsNullOrEmpty(status) || p.PaymentStatus == status), ct);

        var items = pays.Select(p => new PaymentHistoryVm(
            p.PaymentId, p.Amount, p.PaymentMethod, p.PaymentStatus, p.CreatedAt, p.PaidAt, p.Class?.ClassName ?? ""))
            .ToList();

        return new PagedResult<PaymentHistoryVm>(items, total, pageIndex, pageSize);
    }
}

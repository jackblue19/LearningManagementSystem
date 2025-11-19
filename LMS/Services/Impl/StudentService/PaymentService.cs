using System.Linq;
using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Finance;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Services.Impl.StudentService;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IClassRegistrationRepository _registrationRepository;

    private const string RegistrationStatusCancelled = "Cancelled";
    private const string PaymentStatusCompleted = "Completed";

    public PaymentService(
        IPaymentRepository paymentRepository,
        IClassRegistrationRepository registrationRepository)
    {
        _paymentRepository = paymentRepository;
        _registrationRepository = registrationRepository;
    }

    public async Task<Payment?> CreatePaymentAsync(
        Guid studentId,
        Guid classId,
        decimal amount,
        string? paymentMethod = null,
        CancellationToken ct = default)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");
        }

        var registration = await _registrationRepository.FirstOrDefaultAsync(
            registration =>
                registration.StudentId == studentId &&
                registration.ClassId == classId &&
                !string.Equals(registration.RegistrationStatus, RegistrationStatusCancelled, StringComparison.OrdinalIgnoreCase),
            ct: ct);

        if (registration is null) return null;

        var payment = new Payment
        {
            PaymentId = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = classId,
            RegistrationId = registration.RegistrationId,
            Amount = amount,
            PaymentMethod = paymentMethod,
            PaymentStatus = PaymentStatusCompleted,
            CreatedAt = DateTime.UtcNow,
            PaidAt = DateTime.UtcNow
        };

        return await _paymentRepository.AddAsync(payment, ct: ct);
    }

    public Task<IReadOnlyList<Payment>> GetStudentPaymentsAsync(Guid studentId, CancellationToken ct = default)
    {
        Expression<Func<Payment, bool>> predicate = payment => payment.StudentId == studentId;
        return _paymentRepository.ListAsync(
            predicate: predicate,
            orderBy: query => query.OrderByDescending(payment => payment.CreatedAt),
            ct: ct);
    }
}

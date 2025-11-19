using LMS.Models.Entities;

namespace LMS.Services.Interfaces.StudentService;

public interface IPaymentService
{
    Task<Payment?> CreatePaymentAsync(
        Guid studentId,
        Guid classId,
        decimal amount,
        string? paymentMethod = null,
        CancellationToken ct = default);

    Task<IReadOnlyList<Payment>> GetStudentPaymentsAsync(
        Guid studentId,
        CancellationToken ct = default);
}

using LMS.Models.Entities;

namespace LMS.Services.Student;

public interface IStudentBillingService
{
    /// <summary>
    /// Danh sách đăng ký (Approved) của chính student + số tiền phải trả (UnitPrice × TotalSessions).
    /// </summary>
    Task<IReadOnlyList<PayableDto>> ListPayablesAsync(Guid studentId, CancellationToken ct = default);
}

public sealed record PayableDto(
    long RegistrationId,
    Guid ClassId,
    string ClassName,
    decimal UnitPrice,
    int TotalSessions,
    decimal AmountDue,
    string? CenterName);

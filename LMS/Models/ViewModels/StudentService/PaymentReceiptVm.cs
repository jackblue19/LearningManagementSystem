namespace LMS.Models.ViewModels.StudentService;

public sealed record PaymentReceiptVm(
    Guid PaymentId,
    string Status,              // PAID/FAILED/PENDING
    decimal Amount,
    DateTime CreatedAt,
    DateTime? PaidAt,
    string? BankCode,
    string ClassName);

namespace LMS.Models.ViewModels.Bank;

public sealed record PaymentHistoryVm(
    Guid PaymentId,
    decimal Amount,
    string? PaymentMethod,
    string? PaymentStatus,
    DateTime CreatedAt,
    DateTime? PaidAt,
    string ClassName);

namespace LMS.Models.ViewModels.Bank;

public sealed record PaymentCheckoutVm(
    Guid PaymentId,
    long? RegistrationId,
    Guid ClassId,
    decimal Amount,
    string PaymentMethod,
    string PaymentStatus,
    DateTime CreatedAt,
    DateTime? PaidAt,
    string ClassName);

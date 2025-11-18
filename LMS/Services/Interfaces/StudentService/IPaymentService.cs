using LMS.Models.ViewModels;
using LMS.Models.ViewModels.Bank;

namespace LMS.Services.Interfaces.StudentService;

public interface IPaymentService
{
    // Registration_status = "approved"
    Task<PaymentCheckoutVm> CreateRegistrationPaymentAsync(
        Guid studentId, Guid classId, string paymentMethod, CancellationToken ct = default);

    Task<PagedResult<PaymentHistoryVm>> ListMyPaymentsAsync(
        Guid studentId, DateOnly? from = null, DateOnly? to = null, string? status = null,
        int pageIndex = 1, int pageSize = 20, CancellationToken ct = default);
}

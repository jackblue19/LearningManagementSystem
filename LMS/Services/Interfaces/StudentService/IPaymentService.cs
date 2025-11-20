using LMS.Models.ViewModels;
using LMS.Models.ViewModels.Bank;

namespace LMS.Services.Interfaces.StudentService;

public interface IPaymentService
{
    // Registration_status = "approved"
    Task<PaymentCheckoutVm> CreateRegistrationPaymentAsync(
        Guid studentId, Guid classId, string paymentMethod, CancellationToken ct = default);

    Task<bool> UpdateRegistrationPaymentAsync(
        Guid paymentId, string newStatus,
        decimal? amountVerified = null, DateTime? paidAt = null,
        CancellationToken ct = default);

    Task<PagedResult<PaymentHistoryVm>> ListMyPaymentsAsync(
        Guid studentId, DateOnly? from = null, DateOnly? to = null, string? status = null,
        int pageIndex = 1, int pageSize = 20, CancellationToken ct = default);

    // helper
    Guid ParsePaymentIdFromTxnRef(string txnRef);

    //  VNPAY
    // Dùng để redirect người dùng sang VNPAY
    string BuildPaymentUrl(Guid paymentId, decimal amountVnd, string orderInfo, string clientIp);

    // Parse + verify chữ ký từ query trả về (Return, IPN)
    bool TryParseAndVerify(IQueryCollection query, out VnPayReturn data);
}

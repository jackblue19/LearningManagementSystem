using LMS.Models.Entities;
using LMS.Models.ViewModels;

namespace LMS.Services.Student;

public interface IStudentPaymentService
{
    /// <summary>
    /// Lấy danh sách thanh toán của chính student, có phân trang + lọc.
    /// - status: "paid"|"success"|"failed"|"pending" (không phân biệt hoa thường); null = tất cả
    /// - fromUtc/toUtc: mốc thời gian UTC; null = không lọc
    /// </summary>
    Task<PagedResult<Payment>> PagedPaymentsAsync(
        Guid studentId,
        string? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int size,
        CancellationToken ct = default);
}

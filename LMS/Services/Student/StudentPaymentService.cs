using LMS.Models.Entities;
using LMS.Repositories;
using LMS.Models.ViewModels;
using LMS.Services.Interfaces;

namespace LMS.Services.Student;

public sealed class StudentPaymentService : IStudentPaymentService
{
    private readonly ICrudService<Payment, Guid> _payments;

    public StudentPaymentService(ICrudService<Payment, Guid> payments)
    {
        _payments = payments;
    }

    public Task<PagedResult<Payment>> PagedPaymentsAsync(
        Guid studentId,
        string? status,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int size,
        CancellationToken ct = default)
    {
        // Chuẩn hóa status về schema hiện tại (ví dụ: "PAID"|"FAILED"|"PENDING")
        string? normalized = status switch
        {
            null or "" => null,
            var s when s.Equals("paid", StringComparison.OrdinalIgnoreCase)
                    || s.Equals("success", StringComparison.OrdinalIgnoreCase) => "PAID",
            var s when s.Equals("failed", StringComparison.OrdinalIgnoreCase) => "FAILED",
            var s when s.Equals("pending", StringComparison.OrdinalIgnoreCase) => "PENDING",
            _ => status // nếu đã đúng giá trị DB thì giữ nguyên
        };

        // Lọc bằng biểu thức EF (không dùng StringComparison trong LINQ cho EF)
        return _payments.ListAsync(
            predicate: p =>
                p.StudentId == studentId
                && (normalized == null || p.PaymentStatus == normalized)
                && (fromUtc == null || (p.PaidAt != null && p.PaidAt >= fromUtc))
                && (toUtc == null || (p.PaidAt != null && p.PaidAt <= toUtc)),
            orderBy: q => q.OrderByDescending(p => p.PaidAt), // PENDING có thể null -> rớt cuối
            pageIndex: page,
            pageSize: size,
            asNoTracking: true,
            includes: null,
            ct: ct);
    }
}

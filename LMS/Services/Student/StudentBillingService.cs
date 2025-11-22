using LMS.Models.Entities;
using LMS.Repositories;

namespace LMS.Services.Student;

public sealed class StudentBillingService : IStudentBillingService
{
    private readonly IGenericRepository<ClassRegistration, long> _regs;
    private readonly IGenericRepository<Class, Guid> _classes;
    private readonly IGenericRepository<Center, Guid> _centers;

    public StudentBillingService(
        IGenericRepository<ClassRegistration, long> regs,
        IGenericRepository<Class, Guid> classes,
        IGenericRepository<Center, Guid> centers)
    {
        _regs = regs;
        _classes = classes;
        _centers = centers;
    }

    public async Task<IReadOnlyList<PayableDto>> ListPayablesAsync(Guid studentId, CancellationToken ct = default)
    {
        // 1) Các đăng ký đã duyệt của student
        var regs = await _regs.ListAsync(
            r => r.StudentId == studentId && r.RegistrationStatus == "Approved",
            asNoTracking: true, ct: ct);

        if (regs.Count == 0) return Array.Empty<PayableDto>();
        var classIds = regs.Select(r => r.ClassId).Distinct().ToList();

        // 2) Lấy lớp + trung tâm để tính tiền và hiển thị
        var classes = await _classes.ListAsync(c => classIds.Contains(c.ClassId), asNoTracking: true, ct: ct);
        var centers = await _centers.ListAsync(ct: ct, asNoTracking: true);
        var centerMap = centers.ToDictionary(x => x.CenterId, x => x.CenterName ?? string.Empty);

        // 3) Map ra PayableDto
        var classMap = classes.ToDictionary(c => c.ClassId, c => c);
        var items = new List<PayableDto>(regs.Count);

        foreach (var r in regs)
        {
            if (!classMap.TryGetValue(r.ClassId, out var cls)) continue;

            var unit = cls.UnitPrice ?? 0m;
            var sessions = cls.TotalSessions ?? 0;
            var due = unit * sessions;

            items.Add(new PayableDto(
                RegistrationId: r.RegistrationId,
                ClassId: r.ClassId,
                ClassName: cls.ClassName ?? string.Empty,
                UnitPrice: unit,
                TotalSessions: sessions,
                AmountDue: due,
                CenterName: centerMap.TryGetValue(cls.CenterId, out var cn) ? cn : null
            ));
        }

        // Có thể lọc bỏ những đăng ký đã thanh toán đủ nếu sau này bạn nối với bảng Payments/RegistrationId
        return items
            .OrderBy(i => i.CenterName)
            .ThenBy(i => i.ClassName)
            .ToList();
    }
}

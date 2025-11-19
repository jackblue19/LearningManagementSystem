using LMS.Models.Entities;

namespace LMS.Repositories.Interfaces.Academic;

public interface IAttendanceRepository : IGenericRepository<Attendance, long>
{
    Task<bool> HasAttendanceRecordAsync(
        Guid studentId,
        Guid courseId,
        DateTime date,
        CancellationToken ct = default);

    Task<Attendance> GetAttendanceRecordAsync(
        Guid studentId,
        Guid courseId,
        DateTime date,
        CancellationToken ct = default);
}

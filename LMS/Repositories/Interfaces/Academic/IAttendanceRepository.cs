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
    Task<IReadOnlyList<Attendance>> GetAttendancesByScheduleIdAsync(
        long scheduleId,
        CancellationToken ct = default);
    
    Task<Attendance?> GetAttendanceByScheduleAndStudentAsync(
        long scheduleId,
        Guid studentId,
        CancellationToken ct = default);
    
    Task<IReadOnlyList<Attendance>> GetAttendancesByClassIdAsync(
        Guid classId,
        CancellationToken ct = default);
    
    Task BulkUpsertAttendancesAsync(
        IEnumerable<Attendance> attendances,
        CancellationToken ct = default);
}

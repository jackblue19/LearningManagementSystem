using LMS.Models.Entities;

namespace LMS.Services.Interfaces.TeacherService;

public interface IAttendanceService
{
    Task<IReadOnlyList<Attendance>> GetAttendancesByScheduleIdAsync(
        long scheduleId,
        CancellationToken ct = default);

    Task<Attendance> MarkAttendanceAsync(
        long scheduleId,
        Guid studentId,
        string studentStatus,
        string? note = null,
        CancellationToken ct = default);

    Task BulkMarkAttendanceAsync(
        long scheduleId,
        IEnumerable<(Guid StudentId, string Status, string? Note)> attendances,
        CancellationToken ct = default);

    Task<IReadOnlyList<Attendance>> GetAttendancesByClassIdAsync(
        Guid classId,
        CancellationToken ct = default);

    Task<Attendance?> GetAttendanceByScheduleAndStudentAsync(
        long scheduleId,
        Guid studentId,
        CancellationToken ct = default);

    Task<IReadOnlyList<User>> GetStudentsForScheduleAsync(
        long scheduleId,
        CancellationToken ct = default);
}

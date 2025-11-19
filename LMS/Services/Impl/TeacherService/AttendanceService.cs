using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.EntityFrameworkCore;
using LMS.Data;

namespace LMS.Services.Impl.TeacherService;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepo;
    private readonly CenterDbContext _db;

    public AttendanceService(
        IAttendanceRepository attendanceRepo,
        CenterDbContext db)
    {
        _attendanceRepo = attendanceRepo;
        _db = db;
    }

    public async Task<IReadOnlyList<Attendance>> GetAttendancesByScheduleIdAsync(
        long scheduleId,
        CancellationToken ct = default)
    {
        return await _attendanceRepo.GetAttendancesByScheduleIdAsync(scheduleId, ct);
    }

    public async Task<Attendance> MarkAttendanceAsync(
        long scheduleId,
        Guid studentId,
        string studentStatus,
        string? note = null,
        CancellationToken ct = default)
    {
        var existing = await _attendanceRepo.GetAttendanceByScheduleAndStudentAsync(
            scheduleId, studentId, ct);

        if (existing != null)
        {
            existing.StudentStatus = studentStatus;
            existing.Note = note;
            await _attendanceRepo.UpdateAsync(existing, saveNow: true, ct);
            return existing;
        }
        else
        {
            var attendance = new Attendance
            {
                ScheduleId = scheduleId,
                StudentId = studentId,
                StudentStatus = studentStatus,
                Note = note
            };
            return await _attendanceRepo.AddAsync(attendance, saveNow: true, ct);
        }
    }

    public async Task BulkMarkAttendanceAsync(
        long scheduleId,
        IEnumerable<(Guid StudentId, string Status, string? Note)> attendances,
        CancellationToken ct = default)
    {
        var attendanceList = attendances.Select(a => new Attendance
        {
            ScheduleId = scheduleId,
            StudentId = a.StudentId,
            StudentStatus = a.Status,
            Note = a.Note
        });

        await _attendanceRepo.BulkUpsertAttendancesAsync(attendanceList, ct);
    }

    public async Task<IReadOnlyList<Attendance>> GetAttendancesByClassIdAsync(
        Guid classId,
        CancellationToken ct = default)
    {
        return await _attendanceRepo.GetAttendancesByClassIdAsync(classId, ct);
    }

    public async Task<Attendance?> GetAttendanceByScheduleAndStudentAsync(
        long scheduleId,
        Guid studentId,
        CancellationToken ct = default)
    {
        return await _attendanceRepo.GetAttendanceByScheduleAndStudentAsync(
            scheduleId, studentId, ct);
    }

    public async Task<IReadOnlyList<User>> GetStudentsForScheduleAsync(
        long scheduleId,
        CancellationToken ct = default)
    {
        // Get class from schedule
        var schedule = await _db.ClassSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId, ct);

        if (schedule == null)
            return new List<User>();

        // Get students registered for this class
        var students = await _db.ClassRegistrations
            .AsNoTracking()
            .Include(cr => cr.Student)
            .Where(cr => cr.ClassId == schedule.ClassId && 
                   (cr.RegistrationStatus == "approved" || cr.RegistrationStatus == "active"))
            .Select(cr => cr.Student)
            .OrderBy(s => s.FullName)
            .ToListAsync(ct);

        return students!;
    }
}

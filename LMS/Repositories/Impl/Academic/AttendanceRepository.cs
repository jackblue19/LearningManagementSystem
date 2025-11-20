using LMS.Data;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using Microsoft.EntityFrameworkCore;

namespace LMS.Repositories.Impl.Academic;

public class AttendanceRepository : GenericRepository<Attendance, long>, IAttendanceRepository
{
    private readonly CenterDbContext _db;

    public AttendanceRepository(CenterDbContext db) : base(db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Attendance>> GetAttendancesByScheduleIdAsync(
        long scheduleId,
        CancellationToken ct = default)
    {
        return await _db.Attendances
            .AsNoTracking()
            .Where(a => a.ScheduleId == scheduleId)
            .Include(a => a.Student)
            .Include(a => a.Schedule)
            .OrderBy(a => a.Student.FullName)
            .ToListAsync(ct);
    }

    public async Task<Attendance?> GetAttendanceByScheduleAndStudentAsync(
        long scheduleId,
        Guid studentId,
        CancellationToken ct = default)
    {
        return await _db.Attendances
            .Include(a => a.Student)
            .Include(a => a.Schedule)
            .FirstOrDefaultAsync(a => 
                a.ScheduleId == scheduleId && a.StudentId == studentId, ct);
    }

    public async Task<IReadOnlyList<Attendance>> GetAttendancesByClassIdAsync(
        Guid classId,
        CancellationToken ct = default)
    {
        return await _db.Attendances
            .AsNoTracking()
            .Where(a => a.Schedule.ClassId == classId)
            .Include(a => a.Student)
            .Include(a => a.Schedule)
            .OrderBy(a => a.Schedule.SessionDate)
            .ThenBy(a => a.Student.FullName)
            .ToListAsync(ct);
    }

    public async Task BulkUpsertAttendancesAsync(
        IEnumerable<Attendance> attendances,
        CancellationToken ct = default)
    {
        foreach (var attendance in attendances)
        {
            var existing = await _db.Attendances
                .FirstOrDefaultAsync(a => 
                    a.ScheduleId == attendance.ScheduleId && 
                    a.StudentId == attendance.StudentId, ct);

            if (existing != null)
            {
                existing.StudentStatus = attendance.StudentStatus;
                existing.Note = attendance.Note;
                _db.Attendances.Update(existing);
            }
            else
            {
                await _db.Attendances.AddAsync(attendance, ct);
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}

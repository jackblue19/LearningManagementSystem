using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.EntityFrameworkCore;
using LMS.Data;

namespace LMS.Services.Impl.TeacherService;

public class ClassScheduleService : IClassScheduleService
{
    private readonly IClassScheduleRepository _scheduleRepo;
    private readonly CenterDbContext _db;

    public ClassScheduleService(
        IClassScheduleRepository scheduleRepo,
        CenterDbContext db)
    {
        _scheduleRepo = scheduleRepo;
        _db = db;
    }

    public async Task<ClassSchedule?> GetScheduleByIdAsync(
        long scheduleId,
        CancellationToken ct = default)
    {
        return await _db.ClassSchedules
            .AsNoTracking()
            .Include(s => s.Class)
            .Include(s => s.Room)
            .Include(s => s.Slot)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId, ct);
    }

    public async Task<IReadOnlyList<ClassSchedule>> GetSchedulesByClassIdAsync(
        Guid classId,
        CancellationToken ct = default)
    {
        return await _db.ClassSchedules
            .AsNoTracking()
            .Where(s => s.ClassId == classId)
            .Include(s => s.Room)
            .Include(s => s.Slot)
            .OrderBy(s => s.SessionDate)
            .ToListAsync(ct);
    }
}

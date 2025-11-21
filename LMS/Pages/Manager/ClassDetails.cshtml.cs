using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class ClassDetailsModel : PageModel
{
    private readonly CenterDbContext _db;

    public ClassDetailsModel(CenterDbContext db)
    {
        _db = db;
    }

    public Class? ClassInfo { get; set; }
    public List<StudentViewModel> Students { get; set; } = new();
    public List<ScheduleViewModel> UpcomingSchedules { get; set; } = new();
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        ClassInfo = await _db.Classes
            .Include(c => c.Subject)
            .Include(c => c.Teacher)
            .Include(c => c.ClassRegistrations)
            .Include(c => c.ClassSchedules)
            .FirstOrDefaultAsync(c => c.ClassId == id);

        if (ClassInfo == null)
        {
            return NotFound();
        }

        // Load students
        Students = await _db.ClassRegistrations
            .Include(cr => cr.Student)
            .Where(cr => cr.ClassId == id && cr.RegistrationStatus == "approved")
            .Select(cr => new StudentViewModel
            {
                StudentId = cr.StudentId,
                StudentName = cr.Student!.FullName ?? "N/A",
                Email = cr.Student.Email,
                Phone = cr.Student.Phone,
                RegisteredAt = cr.RegisteredAt
            })
            .ToListAsync();

        // Load upcoming schedules (next 10 sessions)
        var today = DateOnly.FromDateTime(DateTime.Now);
        UpcomingSchedules = await _db.ClassSchedules
            .Include(s => s.Room)
            .Where(s => s.ClassId == id && s.SessionDate >= today)
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.StartTime)
            .Take(10)
            .Select(s => new ScheduleViewModel
            {
                SessionDate = s.SessionDate,
                RoomName = s.RoomName ?? "N/A",
                TimeSlot = s.StartTime.HasValue && s.EndTime.HasValue
                    ? $"{s.StartTime.Value.ToString("HH:mm")} - {s.EndTime.Value.ToString("HH:mm")}"
                    : "N/A"
            })
            .ToListAsync();

        // Count sessions
        TotalSessions = ClassInfo.TotalSessions ?? 0;
        CompletedSessions = await _db.ClassSchedules
            .Where(s => s.ClassId == id && s.SessionDate < today)
            .CountAsync();

        return Page();
    }

    public class StudentViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public DateTime RegisteredAt { get; set; }
    }

    public class ScheduleViewModel
    {
        public DateOnly SessionDate { get; set; }
        public string RoomName { get; set; } = "";
        public string TimeSlot { get; set; } = "";
    }
}

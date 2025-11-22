using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager
{
    [Authorize(Policy = "ManagerOnly")]
    public class TeacherAvailabilityModel : PageModel
    {
        private readonly CenterDbContext _db;

        public TeacherAvailabilityModel(CenterDbContext db)
        {
            _db = db;
        }

        public List<TeacherAvailabilityDto> Availabilities { get; set; } = new();
        public List<TeacherSelectDto> Teachers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid? TeacherId { get; set; }

        [BindProperty(SupportsGet = true)]
        public byte? DayOfWeek { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

    public async Task OnGetAsync()
    {
        await LoadTeachersAsync();
        await LoadAvailabilitiesAsync();
    }

        private async Task LoadTeachersAsync()
        {
            Teachers = await _db.Users
                .Where(u => u.RoleDesc == "teacher")
                .OrderBy(u => u.FullName)
                .Select(u => new TeacherSelectDto
                {
                    TeacherId = u.UserId,
                    FullName = u.FullName ?? "N/A"
                })
                .ToListAsync();
        }

    private async Task LoadAvailabilitiesAsync()
    {
        var query = _db.TeacherAvailabilities
            .Include(ta => ta.Teacher)
            .AsQueryable();

        // Filter by teacher
        if (TeacherId.HasValue && TeacherId.Value != Guid.Empty)
        {
            query = query.Where(ta => ta.TeacherId == TeacherId.Value);
        }

        // Filter by day of week
        if (DayOfWeek.HasValue)
        {
            query = query.Where(ta => ta.DayOfWeek == DayOfWeek.Value);
        }

        // Count total
        TotalRecords = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

        // Load data with pagination - without computed properties
        var availabilitiesData = await query
            .OrderBy(ta => ta.DayOfWeek)
            .ThenBy(ta => ta.StartTime)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .Select(ta => new
            {
                ta.AvailabilityId,
                ta.TeacherId,
                TeacherName = ta.Teacher.FullName ?? "N/A",
                ta.DayOfWeek,
                ta.StartTime,
                ta.EndTime,
                ta.Note
            })
            .ToListAsync();

        // Compute properties after loading
        Availabilities = availabilitiesData.Select(ta => new TeacherAvailabilityDto
        {
            AvailabilityId = ta.AvailabilityId,
            TeacherId = ta.TeacherId,
            TeacherName = ta.TeacherName,
            DayOfWeek = ta.DayOfWeek,
            DayOfWeekName = GetDayOfWeekName(ta.DayOfWeek),
            StartTime = ta.StartTime,
            EndTime = ta.EndTime,
            Note = ta.Note,
            HasConflict = CheckConflict(ta.TeacherId, ta.DayOfWeek, ta.StartTime, ta.EndTime)
        }).ToList();
    }

    private bool CheckConflict(Guid teacherId, byte dayOfWeek, TimeOnly startTime, TimeOnly endTime)
    {
        // Check if teacher has any class schedule during this availability
        var hasSchedule = _db.ClassSchedules
            .Include(cs => cs.Slot)
            .Include(cs => cs.Class)
            .Any(cs => cs.Class != null && 
                      cs.Class.TeacherId == teacherId &&
                      cs.SessionDate.DayOfWeek == (DayOfWeek)dayOfWeek &&
                      cs.Slot != null &&
                      cs.Slot.StartTime >= startTime &&
                      cs.Slot.EndTime <= endTime);
        
        return hasSchedule;
    }

    private static string GetDayOfWeekName(byte day)
    {
        return day switch
        {
            1 => "Thứ 2",
            2 => "Thứ 3",
            3 => "Thứ 4",
            4 => "Thứ 5",
            5 => "Thứ 6",
            6 => "Thứ 7",
            0 => "Chủ nhật",
            _ => "N/A"
        };
    }

    public async Task<IActionResult> OnPostDeleteAsync(long availabilityId)
    {
        var availability = await _db.TeacherAvailabilities.FindAsync(availabilityId);
        if (availability == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy slot đăng ký!";
            return RedirectToPage();
        }

        // Check if there are any schedules using this availability
        var hasSchedule = await _db.ClassSchedules
            .Include(cs => cs.Slot)
            .Include(cs => cs.Class)
            .AnyAsync(cs => cs.Class != null && 
                           cs.Class.TeacherId == availability.TeacherId &&
                           cs.SessionDate.DayOfWeek == (DayOfWeek)availability.DayOfWeek &&
                           cs.Slot != null &&
                           cs.Slot.StartTime >= availability.StartTime &&
                           cs.Slot.EndTime <= availability.EndTime);

        if (hasSchedule)
        {
            TempData["ErrorMessage"] = "Không thể xóa slot này vì giáo viên đang có lịch dạy trong khoảng thời gian này!";
            return RedirectToPage();
        }

        _db.TeacherAvailabilities.Remove(availability);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa slot đăng ký thành công!";
        return RedirectToPage();
    }

        public class TeacherAvailabilityDto
        {
            public long AvailabilityId { get; set; }
            public Guid TeacherId { get; set; }
            public string TeacherName { get; set; } = string.Empty;
            public byte DayOfWeek { get; set; }
            public string DayOfWeekName { get; set; } = string.Empty;
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string? Note { get; set; }
            public bool HasConflict { get; set; }
        }

        public class TeacherSelectDto
        {
            public Guid TeacherId { get; set; }
            public string FullName { get; set; } = string.Empty;
        }
    }
}

using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager
{
    [Authorize(Policy = "ManagerOnly")]
    public class TeacherAvailabilityScheduleModel : PageModel
    {
        private readonly CenterDbContext _db;

        public TeacherAvailabilityScheduleModel(CenterDbContext db)
        {
            _db = db;
        }

        public List<TeacherAvailabilityDto> AllAvailabilities { get; set; } = new();
        public List<TimeSlot> TimeSlots { get; set; } = new();
        public List<TeacherSelectDto> Teachers { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid? TeacherId { get; set; }

        public string? SelectedTeacherName { get; set; }

        public async Task OnGetAsync()
        {
            await LoadTeachersAsync();
            await LoadTimeSlotsAsync();
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

            if (TeacherId.HasValue && TeacherId.Value != Guid.Empty)
            {
                SelectedTeacherName = Teachers.FirstOrDefault(t => t.TeacherId == TeacherId.Value)?.FullName;
            }
        }

        private async Task LoadTimeSlotsAsync()
        {
            TimeSlots = await _db.TimeSlots
                .OrderBy(ts => ts.SlotOrder)
                .ToListAsync();
        }

        private async Task LoadAvailabilitiesAsync()
        {
            var query = _db.TeacherAvailabilities
                .Include(ta => ta.Teacher)
                .AsQueryable();

            if (TeacherId.HasValue && TeacherId.Value != Guid.Empty)
            {
                query = query.Where(ta => ta.TeacherId == TeacherId.Value);
            }

            AllAvailabilities = await query
                .Select(ta => new TeacherAvailabilityDto
                {
                    AvailabilityId = ta.AvailabilityId,
                    TeacherId = ta.TeacherId,
                    TeacherName = ta.Teacher.FullName ?? "N/A",
                    DayOfWeek = ta.DayOfWeek,
                    StartTime = ta.StartTime,
                    EndTime = ta.EndTime,
                    Note = ta.Note
                })
                .ToListAsync();
        }

        public List<TeacherAvailabilityDto> GetAvailabilitiesForDay(byte dayOfWeek)
        {
            return AllAvailabilities
                .Where(a => a.DayOfWeek == dayOfWeek)
                .OrderBy(a => a.StartTime)
                .ToList();
        }

        public static string GetDayOfWeekName(byte day)
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

        public bool IsAvailable(byte dayOfWeek, TimeOnly slotStart, TimeOnly slotEnd)
        {
            return AllAvailabilities.Any(a =>
                a.DayOfWeek == dayOfWeek &&
                a.StartTime <= slotStart &&
                a.EndTime >= slotEnd);
        }

        public class TeacherAvailabilityDto
        {
            public long AvailabilityId { get; set; }
            public Guid TeacherId { get; set; }
            public string TeacherName { get; set; } = string.Empty;
            public byte DayOfWeek { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string? Note { get; set; }
        }

        public class TeacherSelectDto
        {
            public Guid TeacherId { get; set; }
            public string FullName { get; set; } = string.Empty;
        }
    }
}

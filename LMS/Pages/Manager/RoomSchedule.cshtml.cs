using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager
{
    [Authorize(Policy = "ManagerOnly")]
    public class RoomScheduleModel : PageModel
    {
        private readonly CenterDbContext _db;

        public RoomScheduleModel(CenterDbContext db)
        {
            _db = db;
        }

        public Room? RoomInfo { get; set; }
        public List<ScheduleDto> Schedules { get; set; } = new();
        public List<TimeSlot> TimeSlots { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateOnly? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly? EndDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ViewType { get; set; } = "list"; // list or grid

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            RoomInfo = await _db.Rooms
                .Include(r => r.Center)
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (RoomInfo == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng học!";
                return RedirectToPage("/Manager/ManageRooms");
            }

            // Set default date range to current week if not specified
            if (!StartDate.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var dayOfWeek = (int)today.DayOfWeek;
                var daysUntilMonday = dayOfWeek == 0 ? -6 : -(dayOfWeek - 1);
                StartDate = today.AddDays(daysUntilMonday);
            }

            if (!EndDate.HasValue)
            {
                EndDate = StartDate.Value.AddDays(6);
            }

            await LoadTimeSlotsAsync();
            await LoadSchedulesAsync(id);

            return Page();
        }

        private async Task LoadTimeSlotsAsync()
        {
            TimeSlots = await _db.TimeSlots
                .OrderBy(ts => ts.SlotOrder)
                .ToListAsync();
        }

        private async Task LoadSchedulesAsync(Guid roomId)
        {
            var schedulesData = await _db.ClassSchedules
                .Include(s => s.Class)
                    .ThenInclude(c => c!.Teacher)
                .Include(s => s.Class)
                    .ThenInclude(c => c!.Subject)
                .Include(s => s.Slot)
                .Where(s => s.RoomId == roomId && 
                           s.SessionDate >= StartDate && 
                           s.SessionDate <= EndDate)
                .OrderBy(s => s.SessionDate)
                .ThenBy(s => s.Slot!.SlotOrder)
                .Select(s => new
                {
                    s.ScheduleId,
                    s.SessionDate,
                    SlotId = s.Slot!.SlotId,
                    SlotOrder = (byte)(s.Slot.SlotOrder ?? 0),
                    StartTime = s.Slot.StartTime,
                    EndTime = s.Slot.EndTime,
                    ClassName = s.Class!.ClassName ?? "N/A",
                    SubjectName = s.Class.Subject!.SubjectName ?? "N/A",
                    TeacherName = s.Class.Teacher!.FullName ?? "N/A",
                    s.RoomName
                })
                .ToListAsync();

            Schedules = schedulesData.Select(s => new ScheduleDto
            {
                ScheduleId = s.ScheduleId,
                SessionDate = s.SessionDate,
                SlotId = s.SlotId,
                SlotOrder = s.SlotOrder,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                ClassName = s.ClassName,
                SubjectName = s.SubjectName,
                TeacherName = s.TeacherName,
                RoomName = s.RoomName ?? "N/A",
                DayOfWeek = s.SessionDate.DayOfWeek
            }).ToList();
        }

        public List<ScheduleDto> GetSchedulesForDate(DateOnly date)
        {
            return Schedules
                .Where(s => s.SessionDate == date)
                .OrderBy(s => s.SlotOrder)
                .ToList();
        }

        public ScheduleDto? GetScheduleForCell(DateOnly date, byte slotOrder)
        {
            return Schedules
                .FirstOrDefault(s => s.SessionDate == date && s.SlotOrder == slotOrder);
        }

        public static string GetDayOfWeekName(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => "N/A"
            };
        }

        public class ScheduleDto
        {
            public long ScheduleId { get; set; }
            public DateOnly SessionDate { get; set; }
            public byte SlotId { get; set; }
            public byte SlotOrder { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public string TeacherName { get; set; } = string.Empty;
            public string RoomName { get; set; } = string.Empty;
            public DayOfWeek DayOfWeek { get; set; }
        }
    }
}

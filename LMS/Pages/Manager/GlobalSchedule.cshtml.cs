using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class GlobalScheduleModel : PageModel
{
    private readonly CenterDbContext _db;

    public GlobalScheduleModel(CenterDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public DateOnly? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ClassId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? RoomId { get; set; }

    public List<ClassSchedule> Schedules { get; set; } = new();
    public List<Class> AvailableClasses { get; set; } = new();
    public List<Room> AvailableRooms { get; set; } = new();
    public List<TimeSlot> TimeSlots { get; set; } = new();

    public int TotalSessions => Schedules.Count;

    public string[] DaysOfWeek = new[]
    {
        "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật"
    };

    public async Task OnGetAsync()
    {
        // Load filter options and time slots
        await LoadFilterOptionsAsync();
        await LoadTimeSlotsAsync();

        // Set default date range if not provided (current week)
        if (!StartDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var dayOfWeek = (int)today.DayOfWeek;
            var daysUntilMonday = dayOfWeek == 0 ? -6 : -(dayOfWeek - 1);
            StartDate = today.AddDays(daysUntilMonday);
        }

        if (!EndDate.HasValue)
        {
            EndDate = StartDate.Value.AddDays(6); // Week ends on Sunday
        }

        // Load schedules
        await LoadSchedulesAsync();
    }

    private async Task LoadFilterOptionsAsync()
    {
        AvailableClasses = await _db.Classes
            .Include(c => c.Subject)
            .Include(c => c.Teacher)
            .OrderBy(c => c.ClassName)
            .ToListAsync();

        AvailableRooms = await _db.Rooms
            .OrderBy(r => r.RoomName)
            .ToListAsync();
    }

    private async Task LoadTimeSlotsAsync()
    {
        TimeSlots = await _db.TimeSlots
            .OrderBy(t => t.SlotOrder)
            .ToListAsync();
    }

    private async Task LoadSchedulesAsync()
    {
        var query = _db.ClassSchedules
            .Include(s => s.Class)
                .ThenInclude(c => c!.Subject)
            .Include(s => s.Class)
                .ThenInclude(c => c!.Teacher)
            .Include(s => s.Room)
            .Include(s => s.Slot)
            .AsQueryable();

        // Apply filters
        if (StartDate.HasValue)
        {
            query = query.Where(s => s.SessionDate >= StartDate.Value);
        }

        if (EndDate.HasValue)
        {
            query = query.Where(s => s.SessionDate <= EndDate.Value);
        }

        if (ClassId.HasValue && ClassId.Value != Guid.Empty)
        {
            query = query.Where(s => s.ClassId == ClassId.Value);
        }

        if (RoomId.HasValue && RoomId.Value != Guid.Empty)
        {
            query = query.Where(s => s.RoomId == RoomId.Value);
        }

        Schedules = await query
            .OrderBy(s => s.SessionDate)
            .ThenBy(s => s.SlotId)
            .ToListAsync();
    }

    public string GetSlotTime(byte slotOrder)
    {
        var slot = TimeSlots.FirstOrDefault(t => t.SlotOrder == slotOrder);
        if (slot != null)
        {
            return $"{slot.StartTime:HH\\:mm} - {slot.EndTime:HH\\:mm}";
        }
        return "N/A";
    }

    public ScheduleCellDto? GetClassForSlot(int weekday, byte slot)
    {
        // Convert weekday (1=Monday, 7=Sunday) to DayOfWeek enum
        DayOfWeek targetDayOfWeek = weekday == 7 ? DayOfWeek.Sunday : (DayOfWeek)weekday;

        var schedulesInSlot = Schedules
            .Where(x => x.SessionDate.DayOfWeek == targetDayOfWeek)
            .Where(x => x.SlotId == slot || x.SlotOrder == slot)
            .ToList();

        var totalRooms = AvailableRooms.Count(r => r.IsActive);

        if (!schedulesInSlot.Any())
        {
            // Không có lịch => tất cả phòng trống
            return new ScheduleCellDto
            {
                Weekday = weekday,
                SlotId = slot,
                SessionDate = StartDate!.Value.AddDays(weekday - 1),
                AvailableRoomsCount = totalRooms,
                TotalRoomsCount = totalRooms
            };
        }

        // Count unique rooms booked
        var bookedRoomsCount = schedulesInSlot
            .Select(x => x.RoomId)
            .Distinct()
            .Count();

        var firstSchedule = schedulesInSlot.First();
        
        return new ScheduleCellDto
        {
            Weekday = weekday,
            SlotId = slot,
            SessionDate = firstSchedule.SessionDate,
            AvailableRoomsCount = totalRooms - bookedRoomsCount,
            TotalRoomsCount = totalRooms
        };
    }

    public async Task<JsonResult> OnGetSlotDetailsAsync(int weekday, byte slotId, string sessionDate)
    {
        var targetDate = DateOnly.Parse(sessionDate);
        
        // Get all schedules for this slot on this date
        var schedulesInSlot = await _db.ClassSchedules
            .Include(s => s.Class)
                .ThenInclude(c => c!.Teacher)
            .Include(s => s.Class)
                .ThenInclude(c => c!.Subject)
            .Include(s => s.Room)
            .Include(s => s.Slot)
            .Where(s => s.SessionDate == targetDate && (s.SlotId == slotId || s.SlotOrder == slotId))
            .OrderBy(s => s.Room!.RoomName)
            .ToListAsync();

        if (!schedulesInSlot.Any())
        {
            return new JsonResult(new { success = false, message = "Không có phòng nào được đặt trong slot này" });
        }

        var slotInfo = schedulesInSlot.First().Slot;
        var bookedRooms = schedulesInSlot.Select(s => new
        {
            RoomName = s.RoomName ?? s.Room?.RoomName ?? "N/A",
            ClassName = s.Class?.ClassName ?? "N/A",
            SubjectName = s.Class?.Subject?.SubjectName ?? "N/A",
            TeacherName = s.Class?.Teacher?.FullName ?? "N/A",
            RoomCapacity = s.Room?.Capacity ?? 0
        }).ToList();

        var totalRooms = await _db.Rooms.CountAsync(r => r.IsActive);

        var result = new
        {
            success = true,
            slotInfo = new
            {
                SlotOrder = slotInfo?.SlotOrder ?? slotId,
                StartTime = slotInfo?.StartTime.ToString("HH:mm") ?? "N/A",
                EndTime = slotInfo?.EndTime.ToString("HH:mm") ?? "N/A",
                SessionDate = targetDate.ToString("dd/MM/yyyy"),
                DayOfWeek = GetDayOfWeekName(targetDate.DayOfWeek)
            },
            bookedRooms = bookedRooms,
            totalRooms = totalRooms,
            bookedRoomsCount = bookedRooms.Count
        };

        return new JsonResult(result);
    }

    private string GetDayOfWeekName(DayOfWeek day)
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

    public class ScheduleCellDto
    {
        public int Weekday { get; set; }
        public byte SlotId { get; set; }
        public DateOnly SessionDate { get; set; }
        public int AvailableRoomsCount { get; set; }
        public int TotalRoomsCount { get; set; }
    }
}

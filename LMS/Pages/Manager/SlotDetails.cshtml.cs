using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class SlotDetailsModel : PageModel
{
    private readonly CenterDbContext _db;

    public SlotDetailsModel(CenterDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int Weekday { get; set; }

    [BindProperty(SupportsGet = true)]
    public byte SlotId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SessionDate { get; set; } = "";

    public DateOnly SessionDateParsed { get; set; }
    public string DayOfWeekName { get; set; } = "";
    public byte SlotOrder { get; set; }
    public string StartTime { get; set; } = "";
    public string EndTime { get; set; } = "";

    public int TotalRooms { get; set; }
    public int AvailableRoomsCount { get; set; }

    public List<BookedRoomDto> BookedRooms { get; set; } = new();
    public List<Room> AvailableRooms { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrEmpty(SessionDate))
        {
            return RedirectToPage("/Manager/GlobalSchedule");
        }

        SessionDateParsed = DateOnly.Parse(SessionDate);
        DayOfWeekName = GetDayOfWeekName(SessionDateParsed.DayOfWeek);

        // Get slot info
        var slot = await _db.TimeSlots.FirstOrDefaultAsync(s => s.SlotId == SlotId);
        if (slot != null)
        {
            SlotOrder = slot.SlotOrder ?? SlotId;
            StartTime = slot.StartTime.ToString("HH:mm");
            EndTime = slot.EndTime.ToString("HH:mm");
        }

        // Get all active rooms
        var allRooms = await _db.Rooms
            .Where(r => r.IsActive)
            .OrderBy(r => r.RoomName)
            .ToListAsync();

        TotalRooms = allRooms.Count;

        // Get booked schedules for this slot
        var schedulesInSlot = await _db.ClassSchedules
            .Include(s => s.Class)
                .ThenInclude(c => c!.Teacher)
            .Include(s => s.Class)
                .ThenInclude(c => c!.Subject)
            .Include(s => s.Room)
            .Where(s => s.SessionDate == SessionDateParsed && 
                       (s.SlotId == SlotId || s.SlotOrder == SlotId))
            .OrderBy(s => s.Room!.RoomName)
            .ToListAsync();

        // Get booked rooms
        BookedRooms = schedulesInSlot.Select(s => new BookedRoomDto
        {
            RoomName = s.RoomName ?? s.Room?.RoomName ?? "N/A",
            RoomCapacity = s.Room?.Capacity ?? 0,
            ClassName = s.Class?.ClassName ?? "N/A",
            SubjectName = s.Class?.Subject?.SubjectName ?? "N/A",
            TeacherName = s.Class?.Teacher?.FullName ?? "N/A"
        }).ToList();

        // Get available rooms (rooms not in booked list)
        var bookedRoomIds = schedulesInSlot
            .Where(s => s.RoomId.HasValue)
            .Select(s => s.RoomId!.Value)
            .ToHashSet();

        AvailableRooms = allRooms
            .Where(r => !bookedRoomIds.Contains(r.RoomId))
            .ToList();

        AvailableRoomsCount = AvailableRooms.Count;

        return Page();
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

    public class BookedRoomDto
    {
        public string RoomName { get; set; } = "";
        public int RoomCapacity { get; set; }
        public string ClassName { get; set; } = "";
        public string SubjectName { get; set; } = "";
        public string TeacherName { get; set; } = "";
    }
}

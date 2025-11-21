using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager
{
    [Authorize(Policy = "ManagerOnly")]
    public class RoomStatusModel : PageModel
    {
        private readonly CenterDbContext _db;

        public RoomStatusModel(CenterDbContext db)
        {
            _db = db;
        }

        public List<RoomStatusDto> RoomStatuses { get; set; } = new();
        public List<TimeSlot> TimeSlots { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public DateOnly? SelectedDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ViewType { get; set; } = "grid"; // grid or timeline

        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }

        public async Task OnGetAsync()
        {
            if (!SelectedDate.HasValue)
            {
                SelectedDate = DateOnly.FromDateTime(DateTime.Now);
            }

            await LoadTimeSlotsAsync();
            await LoadRoomStatusesAsync();
        }

        private async Task LoadTimeSlotsAsync()
        {
            TimeSlots = await _db.TimeSlots
                .OrderBy(ts => ts.SlotOrder)
                .ToListAsync();
        }

        private async Task LoadRoomStatusesAsync()
        {
            // Get all active rooms
            var roomsData = await _db.Rooms
                .Include(r => r.Center)
                .Where(r => r.IsActive)
                .OrderBy(r => r.RoomName)
                .Select(r => new
                {
                    r.RoomId,
                    r.RoomName,
                    r.Capacity,
                    CenterName = r.Center.CenterName ?? "N/A"
                })
                .ToListAsync();

            TotalRooms = roomsData.Count;

            // Get all schedules for selected date
            var schedulesData = await _db.ClassSchedules
                .Include(s => s.Class)
                    .ThenInclude(c => c!.Subject)
                .Include(s => s.Class)
                    .ThenInclude(c => c!.Teacher)
                .Include(s => s.Slot)
                .Where(s => s.SessionDate == SelectedDate)
                .Select(s => new
                {
                    s.RoomId,
                    SlotId = s.Slot!.SlotId,
                    SlotOrder = (byte)(s.Slot.SlotOrder ?? 0),
                    StartTime = s.Slot.StartTime,
                    EndTime = s.Slot.EndTime,
                    ClassName = s.Class!.ClassName ?? "N/A",
                    SubjectName = s.Class.Subject!.SubjectName ?? "N/A",
                    TeacherName = s.Class.Teacher!.FullName ?? "N/A",
                    ClassId = s.Class.ClassId
                })
                .ToListAsync();

            // Build room status list
            RoomStatuses = roomsData.Select(r =>
            {
                var roomSchedules = schedulesData
                    .Where(s => s.RoomId == r.RoomId)
                    .Select(s => new ScheduleSlotDto
                    {
                        SlotId = s.SlotId,
                        SlotOrder = s.SlotOrder,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        ClassName = s.ClassName,
                        SubjectName = s.SubjectName,
                        TeacherName = s.TeacherName,
                        ClassId = s.ClassId
                    })
                    .ToList();

                var currentSlot = GetCurrentTimeSlot();
                var isCurrentlyOccupied = currentSlot != null && 
                    roomSchedules.Any(s => s.SlotOrder == currentSlot.SlotOrder);

                return new RoomStatusDto
                {
                    RoomId = r.RoomId,
                    RoomName = r.RoomName,
                    Capacity = r.Capacity ?? 0,
                    CenterName = r.CenterName,
                    Schedules = roomSchedules,
                    TotalSchedules = roomSchedules.Count,
                    IsCurrentlyOccupied = isCurrentlyOccupied,
                    CurrentClass = isCurrentlyOccupied ? 
                        roomSchedules.FirstOrDefault(s => s.SlotOrder == currentSlot!.SlotOrder)?.ClassName : 
                        null,
                    UtilizationRate = TimeSlots.Count > 0 ? 
                        (int)Math.Round((double)roomSchedules.Count / TimeSlots.Count * 100) : 
                        0
                };
            }).ToList();

            OccupiedRooms = RoomStatuses.Count(r => r.IsCurrentlyOccupied);
            AvailableRooms = TotalRooms - OccupiedRooms;
        }

        private TimeSlot? GetCurrentTimeSlot()
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            return TimeSlots.FirstOrDefault(ts => 
                ts.StartTime <= now && ts.EndTime >= now);
        }

        public ScheduleSlotDto? GetScheduleForSlot(Guid roomId, byte slotOrder)
        {
            var room = RoomStatuses.FirstOrDefault(r => r.RoomId == roomId);
            return room?.Schedules.FirstOrDefault(s => s.SlotOrder == slotOrder);
        }

        public bool IsSlotOccupied(Guid roomId, byte slotOrder)
        {
            return GetScheduleForSlot(roomId, slotOrder) != null;
        }

        public bool IsCurrentSlot(byte slotOrder)
        {
            var currentSlot = GetCurrentTimeSlot();
            return currentSlot != null && currentSlot.SlotOrder == slotOrder;
        }

        public class RoomStatusDto
        {
            public Guid RoomId { get; set; }
            public string RoomName { get; set; } = string.Empty;
            public int Capacity { get; set; }
            public string CenterName { get; set; } = string.Empty;
            public List<ScheduleSlotDto> Schedules { get; set; } = new();
            public int TotalSchedules { get; set; }
            public bool IsCurrentlyOccupied { get; set; }
            public string? CurrentClass { get; set; }
            public int UtilizationRate { get; set; }
        }

        public class ScheduleSlotDto
        {
            public byte SlotId { get; set; }
            public byte SlotOrder { get; set; }
            public TimeOnly StartTime { get; set; }
            public TimeOnly EndTime { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public string SubjectName { get; set; } = string.Empty;
            public string TeacherName { get; set; } = string.Empty;
            public Guid ClassId { get; set; }
        }
    }
}

using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager
{
    [Authorize(Policy = "ManagerOnly")]
    public class ManageRoomsModel : PageModel
    {
        private readonly CenterDbContext _db;

        public ManageRoomsModel(CenterDbContext db)
        {
            _db = db;
        }

        public List<RoomDto> Rooms { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public async Task OnGetAsync()
        {
            await LoadRoomsAsync();
        }

        private async Task LoadRoomsAsync()
        {
            var query = _db.Rooms
                .Include(r => r.Center)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(r => r.RoomName.Contains(Search));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(Status))
            {
                if (Status == "active")
                {
                    query = query.Where(r => r.IsActive == true);
                }
                else if (Status == "inactive")
                {
                    query = query.Where(r => r.IsActive == false);
                }
            }

            // Count total
            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalRecords / (double)PageSize);

            // Load rooms with pagination
            var roomsData = await query
                .OrderBy(r => r.RoomName)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .Select(r => new
                {
                    r.RoomId,
                    r.RoomName,
                    r.Capacity,
                    r.IsActive,
                    CenterName = r.Center.CenterName ?? "N/A"
                })
                .ToListAsync();

            // Calculate usage statistics
            var today = DateOnly.FromDateTime(DateTime.Now);
            var schedulesToday = await _db.ClassSchedules
                .Where(s => s.SessionDate == today)
                .Select(s => s.RoomId)
                .ToListAsync();

            Rooms = roomsData.Select(r => new RoomDto
            {
                RoomId = r.RoomId,
                RoomName = r.RoomName,
                Capacity = r.Capacity ?? 0,
                IsActive = r.IsActive,
                CenterName = r.CenterName,
                IsOccupiedToday = schedulesToday.Contains(r.RoomId),
                TotalSchedules = _db.ClassSchedules.Count(s => s.RoomId == r.RoomId)
            }).ToList();
        }

        public async Task<IActionResult> OnPostDeleteAsync(Guid roomId)
        {
            var room = await _db.Rooms.FindAsync(roomId);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng học!";
                return RedirectToPage();
            }

            // Check if room has schedules
            var hasSchedules = await _db.ClassSchedules.AnyAsync(s => s.RoomId == roomId);
            if (hasSchedules)
            {
                TempData["ErrorMessage"] = "Không thể xóa phòng học vì đã có lịch học sử dụng phòng này!";
                return RedirectToPage();
            }

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Xóa phòng học thành công!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(Guid roomId)
        {
            var room = await _db.Rooms.FindAsync(roomId);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng học!";
                return RedirectToPage();
            }

            room.IsActive = !room.IsActive;
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã {(room.IsActive ? "kích hoạt" : "vô hiệu hóa")} phòng học!";
            return RedirectToPage();
        }

        public class RoomDto
        {
            public Guid RoomId { get; set; }
            public string RoomName { get; set; } = string.Empty;
            public int Capacity { get; set; }
            public bool IsActive { get; set; }
            public string CenterName { get; set; } = string.Empty;
            public bool IsOccupiedToday { get; set; }
            public int TotalSchedules { get; set; }
        }
    }
}

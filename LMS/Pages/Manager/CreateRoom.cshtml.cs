using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LMS.Pages.Manager
{
    [Authorize(Policy = "ManagerOnly")]
    public class CreateRoomModel : PageModel
    {
        private readonly CenterDbContext _db;

        public CreateRoomModel(CenterDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public RoomInputModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get the first center (single center system)
            var center = await _db.Centers.FirstOrDefaultAsync();
            if (center == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin trung tâm trong hệ thống!");
                return Page();
            }

            // Check for duplicate room name
            var existingRoom = await _db.Rooms
                .AnyAsync(r => r.CenterId == center.CenterId && r.RoomName == Input.RoomName);

            if (existingRoom)
            {
                ModelState.AddModelError("Input.RoomName", "Tên phòng đã tồn tại trong trung tâm này!");
                return Page();
            }

            // Validate capacity
            if (Input.Capacity < 1)
            {
                ModelState.AddModelError("Input.Capacity", "Sức chứa phải lớn hơn 0!");
                return Page();
            }

            var room = new Room
            {
                RoomId = Guid.NewGuid(),
                CenterId = center.CenterId,
                RoomName = Input.RoomName.Trim(),
                Capacity = Input.Capacity,
                IsActive = true
            };

            _db.Rooms.Add(room);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Thêm phòng {room.RoomName} thành công!";
            return RedirectToPage("/Manager/ManageRooms");
        }

        public class RoomInputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên phòng")]
            [StringLength(120, ErrorMessage = "Tên phòng không được vượt quá 120 ký tự")]
            public string RoomName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập sức chứa")]
            [Range(1, 1000, ErrorMessage = "Sức chứa phải từ 1 đến 1000")]
            public int Capacity { get; set; } = 30;
        }
    }
}

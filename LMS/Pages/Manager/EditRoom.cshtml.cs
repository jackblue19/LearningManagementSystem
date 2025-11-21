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
    public class EditRoomModel : PageModel
    {
        private readonly CenterDbContext _db;

        public EditRoomModel(CenterDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public RoomInputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var room = await _db.Rooms.FindAsync(id);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phòng học!";
                return RedirectToPage("/Manager/ManageRooms");
            }

            Input = new RoomInputModel
            {
                RoomId = room.RoomId,
                RoomName = room.RoomName,
                Capacity = room.Capacity ?? 0,
                IsActive = room.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var room = await _db.Rooms.FindAsync(Input.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("", "Không tìm thấy phòng học!");
                return Page();
            }

            // Check for duplicate room name (excluding current room)
            var existingRoom = await _db.Rooms
                .AnyAsync(r => r.RoomId != Input.RoomId && 
                              r.CenterId == room.CenterId && 
                              r.RoomName == Input.RoomName);

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

            room.RoomName = Input.RoomName.Trim();
            room.Capacity = Input.Capacity;
            room.IsActive = Input.IsActive;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Cập nhật phòng {room.RoomName} thành công!";
            return RedirectToPage("/Manager/ManageRooms");
        }

        public class RoomInputModel
        {
            public Guid RoomId { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập tên phòng")]
            [StringLength(120, ErrorMessage = "Tên phòng không được vượt quá 120 ký tự")]
            public string RoomName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập sức chứa")]
            [Range(1, 1000, ErrorMessage = "Sức chứa phải từ 1 đến 1000")]
            public int Capacity { get; set; }

            public bool IsActive { get; set; }
        }
    }
}

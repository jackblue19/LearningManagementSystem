using LMS.Models.Entities;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Teacher;

public class TeacherRoomsModel : PageModel
{
    private readonly IRoomService _roomService;

    public TeacherRoomsModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public IReadOnlyList<Room> Rooms { get; set; } = new List<Room>();
    
    [BindProperty(SupportsGet = true)]
    public string? SearchKeyword { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public Guid? CenterId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public int? MinCapacity { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct = default)
    {
        // Search rooms based on filters
        if (!string.IsNullOrWhiteSpace(SearchKeyword) || 
            CenterId.HasValue || 
            MinCapacity.HasValue || 
            IsActive.HasValue)
        {
            Rooms = await _roomService.SearchRoomsAsync(
                keyword: SearchKeyword,
                centerId: CenterId,
                minCapacity: MinCapacity,
                isActive: IsActive,
                ct: ct);
        }
        else
        {
            // Get all active rooms by default
            Rooms = await _roomService.GetAllRoomsAsync(isActive: true, ct: ct);
        }

        return Page();
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid roomId, CancellationToken ct = default)
    {
        var room = await _roomService.GetRoomByIdAsync(roomId, ct);
        if (room == null)
        {
            return NotFound();
        }

        return new JsonResult(new
        {
            roomId = room.RoomId,
            roomName = room.RoomName,
            centerName = room.Center?.CenterName,
            capacity = room.Capacity,
            isActive = room.IsActive
        });
    }
}

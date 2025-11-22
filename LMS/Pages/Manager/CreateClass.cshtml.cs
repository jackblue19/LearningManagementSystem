using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class CreateClassModel : PageModel
{
    private readonly CenterDbContext _db;

    public CreateClassModel(CenterDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ClassInputModel Input { get; set; } = new();

    public List<SelectListItem> Subjects { get; set; } = new();
    public List<SelectListItem> Teachers { get; set; } = new();
    public List<SelectListItem> Rooms { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        try
        {
            // Validate dates
            if (Input.StartDate.HasValue && Input.EndDate.HasValue && Input.StartDate.Value >= Input.EndDate.Value)
            {
                ModelState.AddModelError("", "Ngày kết thúc phải sau ngày bắt đầu!");
                await LoadSelectListsAsync();
                return Page();
            }

            // Get the first available center (project only serves one center)
            var center = await _db.Centers.FirstOrDefaultAsync();
            if (center == null)
            {
                ModelState.AddModelError("", "Không tìm thấy thông tin trung tâm trong hệ thống!");
                await LoadSelectListsAsync();
                return Page();
            }

            // Validate teacher exists if selected
            if (Input.TeacherId.HasValue && Input.TeacherId.Value != Guid.Empty)
            {
                var teacherExists = await _db.Users.AnyAsync(u => u.UserId == Input.TeacherId.Value);
                if (!teacherExists)
                {
                    ModelState.AddModelError("", "Giáo viên không tồn tại!");
                    await LoadSelectListsAsync();
                    return Page();
                }
            }

            var newClass = new Class
            {
                ClassId = Guid.NewGuid(),
                ClassName = Input.ClassName,
                SubjectId = Input.SubjectId,
                TeacherId = Input.TeacherId.HasValue && Input.TeacherId.Value != Guid.Empty ? Input.TeacherId.Value : Guid.Empty,
                StartDate = Input.StartDate,
                EndDate = Input.EndDate,
                TotalSessions = Input.TotalSessions,
                UnitPrice = Input.UnitPrice,
                ScheduleDesc = Input.Description,
                CenterId = center.CenterId,
                ClassStatus = "active",
                CreatedAt = DateTime.Now
            };

            _db.Classes.Add(newClass);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo lớp học thành công!";
            return RedirectToPage("/Manager/ManageClasses");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            await LoadSelectListsAsync();
            return Page();
        }
    }

    private async Task LoadSelectListsAsync()
    {
        Subjects = await _db.Subjects
            .Select(s => new SelectListItem
            {
                Value = s.SubjectId.ToString(),
                Text = s.SubjectName
            })
            .ToListAsync();

        Teachers = await _db.Users
            .Where(u => u.RoleDesc == "Teacher" || u.RoleDesc == "teacher")
            .Select(u => new SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = u.FullName
            })
            .ToListAsync();

        Rooms = await _db.Rooms
            .Select(r => new SelectListItem
            {
                Value = r.RoomId.ToString(),
                Text = $"{r.RoomName} (SL: {r.Capacity})"
            })
            .ToListAsync();
    }

    public class ClassInputModel
    {
        public string ClassName { get; set; } = "";
        public long SubjectId { get; set; }
        public Guid? TeacherId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int? TotalSessions { get; set; }
        public decimal? UnitPrice { get; set; }
        public string? Description { get; set; }
    }
}

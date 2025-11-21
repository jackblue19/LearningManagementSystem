using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class EditClassModel : PageModel
{
    private readonly CenterDbContext _db;

    public EditClassModel(CenterDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public ClassInputModel Input { get; set; } = new();

    public List<SelectListItem> Subjects { get; set; } = new();
    public List<SelectListItem> Teachers { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var classEntity = await _db.Classes
            .Include(c => c.Subject)
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.ClassId == id);

        if (classEntity == null)
        {
            return NotFound();
        }

        Input = new ClassInputModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,
            SubjectId = classEntity.SubjectId,
            TeacherId = classEntity.TeacherId != Guid.Empty ? classEntity.TeacherId : null,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            TotalSessions = classEntity.TotalSessions,
            UnitPrice = classEntity.UnitPrice,
            Description = classEntity.ScheduleDesc
        };

        await LoadSelectListsAsync();
        return Page();
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
            var classEntity = await _db.Classes.FindAsync(Input.ClassId);
            if (classEntity == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lớp học!";
                return RedirectToPage("/Manager/ManageClasses");
            }

            // Validate dates
            if (Input.StartDate.HasValue && Input.EndDate.HasValue && Input.StartDate.Value >= Input.EndDate.Value)
            {
                ModelState.AddModelError("", "Ngày kết thúc phải sau ngày bắt đầu!");
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

            // Update class properties
            classEntity.ClassName = Input.ClassName;
            classEntity.SubjectId = Input.SubjectId;
            classEntity.TeacherId = Input.TeacherId.HasValue && Input.TeacherId.Value != Guid.Empty 
                ? Input.TeacherId.Value 
                : Guid.Empty;
            classEntity.StartDate = Input.StartDate;
            classEntity.EndDate = Input.EndDate;
            classEntity.TotalSessions = Input.TotalSessions;
            classEntity.UnitPrice = Input.UnitPrice;
            classEntity.ScheduleDesc = Input.Description;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Cập nhật lớp học thành công!";
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
    }

    public class ClassInputModel
    {
        public Guid ClassId { get; set; }
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

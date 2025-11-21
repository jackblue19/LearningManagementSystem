using LMS.Data;
using LMS.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class ManageClassesModel : PageModel
{
    private readonly CenterDbContext _db;

    public ManageClassesModel(CenterDbContext db)
    {
        _db = db;
    }

    public List<ClassViewModel> Classes { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalPages { get; set; }
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int pageNumber = 1, string? search = null, string? status = null)
    {
        PageNumber = pageNumber;
        SearchTerm = search;
        StatusFilter = status;

        if (TempData["SuccessMessage"] != null)
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
        if (TempData["ErrorMessage"] != null)
            ErrorMessage = TempData["ErrorMessage"]?.ToString();

        var today = DateOnly.FromDateTime(DateTime.Now);

        var query = _db.Classes
            .Include(c => c.Subject)
            .Include(c => c.Teacher)
            .Include(c => c.ClassRegistrations)
            .AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.ClassName.Contains(search) ||
                                   (c.Teacher != null && c.Teacher.FullName != null && c.Teacher.FullName.Contains(search)));
        }

        // Status filter
        if (!string.IsNullOrEmpty(status))
        {
            query = status switch
            {
                "upcoming" => query.Where(c => c.StartDate.HasValue && c.StartDate.Value > today),
                "active" => query.Where(c => c.StartDate.HasValue && c.EndDate.HasValue &&
                                           c.StartDate.Value <= today && c.EndDate.Value >= today),
                "completed" => query.Where(c => c.EndDate.HasValue && c.EndDate.Value < today),
                _ => query
            };
        }

        var totalCount = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        Classes = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .Select(c => new ClassViewModel
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                SubjectName = c.Subject != null ? c.Subject.SubjectName : "N/A",
                TeacherName = c.Teacher != null ? c.Teacher.FullName : "Chưa phân công",
                TeacherId = c.TeacherId,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                TotalSessions = c.TotalSessions ?? 0,
                UnitPrice = c.UnitPrice ?? 0,
                TotalStudents = c.ClassRegistrations.Count(cr => cr.RegistrationStatus == "approved"),
                Status = c.StartDate.HasValue && c.EndDate.HasValue
                    ? (c.StartDate.Value > today ? "upcoming" :
                       c.EndDate.Value < today ? "completed" : "active")
                    : "draft"
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid classId)
    {
        try
        {
            var classEntity = await _db.Classes
                .Include(c => c.ClassRegistrations)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            if (classEntity == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lớp học!";
                return RedirectToPage();
            }

            // Check if class has students
            if (classEntity.ClassRegistrations.Any(cr => cr.RegistrationStatus == "approved"))
            {
                TempData["ErrorMessage"] = "Không thể xóa lớp đã có học sinh đăng ký!";
                return RedirectToPage();
            }

            _db.Classes.Remove(classEntity);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa lớp học thành công!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi khi xóa lớp học: {ex.Message}";
        }

        return RedirectToPage();
    }

    public class ClassViewModel
    {
        public Guid ClassId { get; set; }
        public string ClassName { get; set; } = "";
        public string SubjectName { get; set; } = "";
        public string TeacherName { get; set; } = "";
        public Guid TeacherId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public int TotalSessions { get; set; }
        public decimal UnitPrice { get; set; }
        public int TotalStudents { get; set; }
        public string Status { get; set; } = "";
    }
}

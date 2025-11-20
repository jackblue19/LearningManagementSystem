using LMS.Models.Entities;
using LMS.Services.Interfaces.TeacherService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Teacher;

public class TeacherAttendanceModel : PageModel
{
    private readonly IAttendanceService _attendanceService;
    private readonly IClassScheduleService _scheduleService;

    public TeacherAttendanceModel(
        IAttendanceService attendanceService,
        IClassScheduleService scheduleService)
    {
        _attendanceService = attendanceService;
        _scheduleService = scheduleService;
    }

    public ClassSchedule? Schedule { get; set; }
    public IReadOnlyList<User> Students { get; set; } = new List<User>();
    public IReadOnlyList<Attendance> Attendances { get; set; } = new List<Attendance>();
    
    [BindProperty]
    public long ScheduleId { get; set; }
    
    [BindProperty]
    public List<AttendanceInput> AttendanceInputs { get; set; } = new();

    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(long? scheduleId, CancellationToken ct = default)
    {
        if (!scheduleId.HasValue)
        {
            ErrorMessage = "Vui lòng chọn buổi học để điểm danh.";
            return Page();
        }

        ScheduleId = scheduleId.Value;

        // Get schedule details
        Schedule = await _scheduleService.GetScheduleByIdAsync(scheduleId.Value, ct);

        if (Schedule == null)
        {
            ErrorMessage = "Không tìm thấy buổi học.";
            return Page();
        }

        // Get students for this schedule
        Students = await _attendanceService.GetStudentsForScheduleAsync(scheduleId.Value, ct);

        // Get existing attendances
        Attendances = await _attendanceService.GetAttendancesByScheduleIdAsync(scheduleId.Value, ct);

        // Initialize attendance inputs
        AttendanceInputs = Students.Select(s =>
        {
            var existing = Attendances.FirstOrDefault(a => a.StudentId == s.UserId);
            return new AttendanceInput
            {
                StudentId = s.UserId,
                StudentName = s.FullName ?? s.Username,
                Status = existing?.StudentStatus ?? "present",
                Note = existing?.Note
            };
        }).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct = default)
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Dữ liệu không hợp lệ.";
            return Page();
        }

        try
        {
            var attendances = AttendanceInputs.Select(a => 
                (a.StudentId, a.Status, a.Note)).ToList();

            await _attendanceService.BulkMarkAttendanceAsync(
                ScheduleId, 
                attendances, 
                ct);

            Message = "Điểm danh thành công!";
            return RedirectToPage(new { scheduleId = ScheduleId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi điểm danh: {ex.Message}";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostQuickMarkAsync(
        long scheduleId, 
        string markAll, 
        CancellationToken ct = default)
    {
        try
        {
            var students = await _attendanceService.GetStudentsForScheduleAsync(scheduleId, ct);
            var attendances = students.Select(s => 
                (s.UserId, markAll, (string?)null)).ToList();

            await _attendanceService.BulkMarkAttendanceAsync(scheduleId, attendances, ct);

            Message = $"Đã đánh dấu tất cả học viên là '{markAll}'.";
            return RedirectToPage(new { scheduleId });
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi: {ex.Message}";
            return RedirectToPage(new { scheduleId });
        }
    }

    public class AttendanceInput
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Status { get; set; } = "present";
        public string? Note { get; set; }
    }
}

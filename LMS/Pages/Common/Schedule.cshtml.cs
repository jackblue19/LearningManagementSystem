using System.ComponentModel.DataAnnotations;
using LMS.Models.Entities;
using LMS.Services.Interfaces.StudentService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Common;

public class ScheduleModel : PageModel
{
    private readonly IStudentScheduleService _scheduleService;

    public ScheduleModel(IStudentScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? EndDate { get; set; }

    public IReadOnlyList<ClassSchedule> Schedules { get; private set; } = Array.Empty<ClassSchedule>();

    public int TotalSessions => Schedules.Count;

    public string[] DaysOfWeek = new[]
    {
        "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7", "Chủ nhật"
    };

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (StudentId == Guid.Empty) return;
        Schedules = await _scheduleService.GetScheduleAsync(StudentId, StartDate, EndDate, ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (StudentId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(StudentId), "Student Id is required.");
            return Page();
        }

        Schedules = await _scheduleService.GetScheduleAsync(StudentId, StartDate, EndDate, ct);
        return Page();
    }

    public string GetSlotTime(int slot) => slot switch
    {
        1 => "07:00 - 08:30",
        2 => "09:00 - 10:30",
        3 => "13:30 - 15:00",
        4 => "15:30 - 17:00",
        5 => "17:30 - 19:00",
        6 => "19:30 - 21:00",
        _ => ""
    };

    public ScheduleCellDto? GetClassForSlot(int weekday, int slot)
    {
        // Convert weekday (1=Monday, 7=Sunday) to DayOfWeek enum
        // DayOfWeek: Sunday=0, Monday=1, Tuesday=2, ..., Saturday=6
        // Our weekday: Monday=1, Tuesday=2, ..., Sunday=7
        DayOfWeek targetDayOfWeek = weekday == 7 ? DayOfWeek.Sunday : (DayOfWeek)weekday;

        return Schedules
            .Where(x => x.SessionDate.DayOfWeek == targetDayOfWeek)
            .Where(x => x.SlotId == slot || x.SlotOrder == slot)
            .Select(x => new ScheduleCellDto
            {
                ClassName = x.Class?.ClassName ?? "",
                RoomName = x.RoomName ?? x.Room?.RoomName ?? "",
                TeacherName = x.Class?.Teacher?.FullName ?? ""
            })
            .FirstOrDefault();
    }

    public class ScheduleCellDto
    {
        public string ClassName { get; set; } = "";
        public string RoomName { get; set; } = "";
        public string TeacherName { get; set; } = "";
    }
}

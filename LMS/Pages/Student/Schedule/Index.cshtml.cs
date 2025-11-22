using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LMS.Pages._Base;
using LMS.Services.Student;

namespace LMS.Pages.Student.Schedule;

[Authorize(Roles = "student")]
public sealed class IndexModel : AppPageModel
{
    private readonly IStudentScheduleService _svc;
    public IndexModel(IStudentScheduleService svc) => _svc = svc;

    [BindProperty(SupportsGet = true)] public string? Range { get; set; } = "week"; // week|month
    [BindProperty(SupportsGet = true)] public DateOnly? From { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ClassId { get; set; }

    public IReadOnlyList<StudentScheduleItem> Items { get; private set; } = Array.Empty<StudentScheduleItem>();
    public IReadOnlyList<MyClassItem> MyClasses { get; private set; } = Array.Empty<MyClassItem>();
    public DateOnly FromDate { get; private set; }
    public DateOnly ToDate { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var from = From ?? today;
        var to = Range?.ToLowerInvariant() switch
        {
            "month" => AddMonthsClamp(from, 1).AddDays(-1),
            _ => from.AddDays(6)
        };
        FromDate = from; ToDate = to;

        var studentId = CurrentUserId;

        // dropdown lớp
        MyClasses = await _svc.ListMyClassesAsync(studentId, ct);

        // lịch
        Items = await _svc.ListSchedulesByStudentAsync(studentId, from, to, ClassId, ct);
    }

    private static DateOnly AddMonthsClamp(DateOnly d, int months)
    {
        var newMonth = d.Month + months;
        var year = d.Year + (newMonth - 1) / 12;
        var month = ((newMonth - 1) % 12) + 1;
        var day = Math.Min(d.Day, DateTime.DaysInMonth(year, month));
        return new DateOnly(year, month, day);
    }
}

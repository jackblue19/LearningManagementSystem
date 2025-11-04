using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Areas.Admin.Pages.Exams;

public class IndexModel : PageModel
{
    public IReadOnlyList<ExamScheduleViewModel> Schedule { get; private set; } = Array.Empty<ExamScheduleViewModel>();
    public IReadOnlyList<ExamResultSummaryViewModel> Results { get; private set; } = Array.Empty<ExamResultSummaryViewModel>();
    public IReadOnlyList<AttendanceRecordViewModel> Attendance { get; private set; } = Array.Empty<AttendanceRecordViewModel>();
    public IReadOnlyList<NotificationViewModel> Notifications { get; private set; } = Array.Empty<NotificationViewModel>();
    public IReadOnlyList<AuditLogEntryViewModel> AuditLog { get; private set; } = Array.Empty<AuditLogEntryViewModel>();

    public int UpcomingCount => Schedule.Count(e => e.ScheduledAt >= DateTime.UtcNow && e.ScheduledAt <= DateTime.UtcNow.AddDays(14));
    public int CompletedCount => Schedule.Count(e => e.ScheduledAt < DateTime.UtcNow && string.Equals(e.Status, "Completed", StringComparison.OrdinalIgnoreCase));
    public decimal AverageExamScore => Results.Count == 0 ? 0 : Math.Round(Results.Average(r => r.AverageScore), 1);
    public decimal AveragePassRate => Results.Count == 0 ? 0 : Math.Round(Results.Average(r => r.PassRate), 1);

    public void OnGet()
    {
        Schedule = new List<ExamScheduleViewModel>
        {
            new("Data mining certification", "Applied Machine Learning", DateTime.UtcNow.AddDays(4).AddHours(10), "Innovation Lab", "Scheduled"),
            new("Narrative design midterm", "Narrative Design", DateTime.UtcNow.AddDays(7).AddHours(9), "Studio B", "Scheduled"),
            new("Leadership capstone", "Operations Leadership", DateTime.UtcNow.AddDays(-2).AddHours(14), "Lecture Hall 3", "Completed"),
            new("Calculus mastery exam", "Advanced Mathematics", DateTime.UtcNow.AddDays(2).AddHours(13), "Lecture Hall 2", "Preparing")
        };

        Results = new List<ExamResultSummaryViewModel>
        {
            new("Leadership capstone", "Executive cohort", 86.4m, 92.3m),
            new("Data mining certification", "Graduate", 78.1m, 88.5m),
            new("Narrative design midterm", "Creative", 81.7m, 90.0m)
        };

        Attendance = new List<AttendanceRecordViewModel>
        {
            new(DateTime.UtcNow.AddDays(-1), "Leadership capstone", 48, 2),
            new(DateTime.UtcNow.AddDays(-3), "Narrative design midterm", 26, 1)
        };

        Notifications = new List<NotificationViewModel>
        {
            new("Exam proctor briefing", "Operations", "Apr 02, 07:30", "Scheduled"),
            new("Exam results published", "Executive cohort", "Mar 31, 16:00", "Sent"),
            new("Materials upload reminder", "Faculty", "Mar 30, 10:00", "Sent")
        };

        AuditLog = new List<AuditLogEntryViewModel>
        {
            new(DateTime.UtcNow.AddHours(-3), "Alicia Graham", "Published results for", "Leadership capstone", "Web"),
            new(DateTime.UtcNow.AddHours(-6), "Rahul Patel", "Updated timetable", "Calculus mastery exam", "Web"),
            new(DateTime.UtcNow.AddHours(-12), "Exams API", "Synced rosters", "Data mining certification", "API")
        };
    }
}

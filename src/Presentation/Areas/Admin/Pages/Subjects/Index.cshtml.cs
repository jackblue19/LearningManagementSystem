using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Areas.Admin.Pages.Subjects;

public class IndexModel : PageModel
{
    public IReadOnlyList<SubjectOverviewViewModel> Subjects { get; private set; } = Array.Empty<SubjectOverviewViewModel>();
    public IReadOnlyList<ClassMaterialViewModel> Materials { get; private set; } = Array.Empty<ClassMaterialViewModel>();
    public IReadOnlyList<AttendanceRecordViewModel> Attendance { get; private set; } = Array.Empty<AttendanceRecordViewModel>();
    public IReadOnlyList<NotificationViewModel> Notifications { get; private set; } = Array.Empty<NotificationViewModel>();
    public IReadOnlyList<TeacherAvailabilityViewModel> Teachers { get; private set; } = Array.Empty<TeacherAvailabilityViewModel>();

    public int TotalModules => Subjects.Sum(s => s.ModuleCount);
    public int RecentlyUpdated => Subjects.Count(s => s.LastUpdated.Contains("2025"));
    public int UniqueOwners => Subjects.Select(s => s.Owner).Distinct(StringComparer.OrdinalIgnoreCase).Count();

    public void OnGet()
    {
        Subjects = new List<SubjectOverviewViewModel>
        {
            new("Applied Machine Learning", "SUB-201", 8, "Alicia Graham", "Apr 02, 2025"),
            new("Narrative Design", "SUB-318", 6, "Morgan Lee", "Mar 28, 2025"),
            new("Operations Leadership", "SUB-450", 5, "Amelia Brown", "Apr 01, 2025"),
            new("Advanced Mathematics", "SUB-105", 10, "Rahul Patel", "Mar 25, 2025")
        };

        Materials = new List<ClassMaterialViewModel>
        {
            new("ML lab checklist", "PDF", "Alicia Graham", DateTime.UtcNow.AddDays(-3)),
            new("Storytelling workbook", "Document", "Morgan Lee", DateTime.UtcNow.AddDays(-6)),
            new("Leadership scenarios", "Slide", "Amelia Brown", DateTime.UtcNow.AddDays(-4))
        };

        Attendance = new List<AttendanceRecordViewModel>
        {
            new(DateTime.UtcNow.Date.AddDays(-1), "Applied Machine Learning", 24, 2),
            new(DateTime.UtcNow.Date.AddDays(-2), "Operations Leadership", 20, 1),
            new(DateTime.UtcNow.Date.AddDays(-3), "Advanced Mathematics", 22, 4)
        };

        Notifications = new List<NotificationViewModel>
        {
            new("Curriculum review window", "Subject owners", "Apr 01, 08:30", "Scheduled"),
            new("Assessment updates", "All faculty", "Mar 30, 17:45", "Sent"),
            new("Subject catalog export", "Admin team", "Mar 29, 09:15", "Sent")
        };

        Teachers = new List<TeacherAvailabilityViewModel>
        {
            new("Alicia Graham", "Data Science", "Mon - Thu", "Available"),
            new("Rahul Patel", "Mathematics", "Tue - Fri", "Available"),
            new("Amelia Brown", "Leadership", "Wed - Fri", "Reviewing")
        };
    }
}

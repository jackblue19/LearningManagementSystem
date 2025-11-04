using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Areas.Admin.Pages.Classes;

public class IndexModel : PageModel
{
    public IReadOnlyList<ClassOverviewViewModel> Classes { get; private set; } = Array.Empty<ClassOverviewViewModel>();
    public IReadOnlyList<ClassMaterialViewModel> Materials { get; private set; } = Array.Empty<ClassMaterialViewModel>();
    public IReadOnlyList<RegistrationSnapshotViewModel> Registrations { get; private set; } = Array.Empty<RegistrationSnapshotViewModel>();
    public IReadOnlyList<RoomAvailabilityViewModel> Rooms { get; private set; } = Array.Empty<RoomAvailabilityViewModel>();
    public IReadOnlyList<TeacherAvailabilityViewModel> Teachers { get; private set; } = Array.Empty<TeacherAvailabilityViewModel>();
    public IReadOnlyList<ScheduleBatchViewModel> ScheduleBatches { get; private set; } = Array.Empty<ScheduleBatchViewModel>();

    public int ActiveClasses => Classes.Count(c => string.Equals(c.Status, "Active", StringComparison.OrdinalIgnoreCase));
    public int UpcomingSessions => Classes.Count(c => c.Status.Contains("Upcoming", StringComparison.OrdinalIgnoreCase));
    public int AverageEnrollment => Classes.Count == 0 ? 0 : (int)Math.Round(Classes.Average(c => c.Enrolled));
    public decimal CapacityUtilization
    {
        get
        {
            var totals = Registrations.Where(r => r.Capacity > 0).ToList();
            if (totals.Count == 0)
            {
                return 0;
            }

            var registered = totals.Sum(r => r.Registered);
            var capacity = totals.Sum(r => r.Capacity);
            return capacity == 0 ? 0 : Math.Round(registered * 100m / capacity, 0);
        }
    }

    public void OnGet()
    {
        Classes = new List<ClassOverviewViewModel>
        {
            new("Foundations of Data Science", "CLS-101", "Alicia Graham", 28, "Mon, 10:00 AM", "Active"),
            new("Advanced Calculus Mentoring", "CLS-214", "Rahul Patel", 22, "Tue, 14:00 PM", "Active"),
            new("Creative Writing Studio", "CLS-330", "Morgan Lee", 18, "Wed, 09:00 AM", "Upcoming"),
            new("Leadership Practicum", "CLS-440", "Amelia Brown", 26, "Thu, 11:30 AM", "Active")
        };

        Materials = new List<ClassMaterialViewModel>
        {
            new("Data pipelines workbook", "PDF", "Alicia Graham", DateTime.UtcNow.AddDays(-2)),
            new("Calculus sprint deck", "Slide", "Rahul Patel", DateTime.UtcNow.AddDays(-5)),
            new("Studio prompts", "Document", "Morgan Lee", DateTime.UtcNow.AddDays(-1))
        };

        Registrations = new List<RegistrationSnapshotViewModel>
        {
            new("Foundations of Data Science", "Spring 2025", 28, 30),
            new("Advanced Calculus Mentoring", "Year 2", 22, 24),
            new("Leadership Practicum", "Executive", 26, 28)
        };

        Rooms = new List<RoomAvailabilityViewModel>
        {
            new("Innovation Lab", "36 seats", "08:00 - 12:00", "Available"),
            new("Studio B", "24 seats", "13:00 - 17:00", "In use"),
            new("Lecture Hall 4", "120 seats", "All day", "Maintenance")
        };

        Teachers = new List<TeacherAvailabilityViewModel>
        {
            new("Alicia Graham", "Data Science", "Mon - Thu", "Available"),
            new("Rahul Patel", "Mathematics", "Tue - Fri", "Available"),
            new("Morgan Lee", "Writing", "Wed - Thu", "Adjusting")
        };

        ScheduleBatches = new List<ScheduleBatchViewModel>
        {
            new("Spring refresh", "Apr 1 - Apr 30", "Operations", "Planning"),
            new("Summer intensive", "Jun 10 - Jun 30", "Academic ops", "Draft"),
            new("Faculty onboarding", "Aug 1 - Aug 7", "HR", "Confirmed")
        };
    }
}

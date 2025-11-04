using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Areas.Teachers.Pages;

public class ManageClassModel : PageModel
{
    public IReadOnlyList<ClassSummary> Classes { get; private set; } = Array.Empty<ClassSummary>();

    public IReadOnlyList<ScheduleItem> UpcomingSessions { get; private set; } = Array.Empty<ScheduleItem>();

    public IReadOnlyList<RegistrationRequest> RegistrationQueue { get; private set; } = Array.Empty<RegistrationRequest>();

    public int TotalClasses => Classes.Count;

    public int ActiveClasses => Classes.Count(c => c.State is ClassState.InSession or ClassState.EnrollmentOpen);

    public int LearnersEnrolled => Classes.Sum(c => c.Enrollment);

    public double AverageAttendance => Classes.Count == 0 ? 0 : Math.Round(Classes.Average(c => c.AttendanceRate), 1);

    public double CapacityUtilisation
    {
        get
        {
            var totalCapacity = Classes.Sum(c => c.Capacity);
            if (totalCapacity == 0)
            {
                return 0;
            }

            return Math.Round(LearnersEnrolled / (double)totalCapacity * 100, 1);
        }
    }

    public int PendingRegistrations => RegistrationQueue.Count(r => r.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase));

    public int WaitlistedHeadcount => RegistrationQueue.Count(r => r.Status.Equals("Waitlisted", StringComparison.OrdinalIgnoreCase));

    public int SessionsThisWeek => UpcomingSessions.Count(s => s.Start.Date <= DateTime.Today.AddDays(7));

    public ScheduleItem? NextSession => UpcomingSessions.OrderBy(s => s.Start).FirstOrDefault();

    public void OnGet()
    {
        var today = DateTime.Today;

        Classes = new List<ClassSummary>
        {
            new(
                Name: "Modern Web Apps",
                Code: "WEB-302",
                Subject: "Advanced Frontend Engineering",
                Instructor: "Amelia Benson",
                Room: "Innovation Lab 2B",
                Enrollment: 24,
                Capacity: 28,
                AttendanceRate: 92.5,
                NextSession: today.AddDays(1).AddHours(9),
                State: ClassState.InSession,
                LastUpdated: today.AddDays(-1).AddHours(16)),
            new(
                Name: "Data Storytelling",
                Code: "ANA-210",
                Subject: "Visual Analytics",
                Instructor: "Mason Ruiz",
                Room: "Room 401",
                Enrollment: 18,
                Capacity: 20,
                AttendanceRate: 88.0,
                NextSession: today.AddDays(2).AddHours(13),
                State: ClassState.EnrollmentOpen,
                LastUpdated: today.AddHours(-6)),
            new(
                Name: "AI Fundamentals",
                Code: "AI-101",
                Subject: "Applied Machine Learning",
                Instructor: "Priya Nair",
                Room: "Hybrid - Teams",
                Enrollment: 30,
                Capacity: 30,
                AttendanceRate: 96.2,
                NextSession: today.AddDays(3).AddHours(10),
                State: ClassState.Waitlisted,
                LastUpdated: today.AddDays(-2).AddHours(11)),
            new(
                Name: "Leadership Lab",
                Code: "LDR-115",
                Subject: "Collaborative Leadership",
                Instructor: "Noah Adams",
                Room: "Studio 205",
                Enrollment: 15,
                Capacity: 18,
                AttendanceRate: 91.4,
                NextSession: today.AddDays(4).AddHours(15),
                State: ClassState.InSession,
                LastUpdated: today.AddDays(-1).AddHours(9))
        };

        UpcomingSessions = new List<ScheduleItem>
        {
            new(
                ClassName: "Modern Web Apps",
                Start: today.AddDays(1).AddHours(9),
                End: today.AddDays(1).AddHours(11),
                Room: "Innovation Lab 2B",
                Focus: "Sprint retrospectives",
                IsOnline: false),
            new(
                ClassName: "Data Storytelling",
                Start: today.AddDays(2).AddHours(13),
                End: today.AddDays(2).AddHours(15),
                Room: "Room 401",
                Focus: "Interactive dashboards",
                IsOnline: false),
            new(
                ClassName: "AI Fundamentals",
                Start: today.AddDays(3).AddHours(10),
                End: today.AddDays(3).AddHours(12),
                Room: "Teams Live",
                Focus: "Model evaluation clinic",
                IsOnline: true),
            new(
                ClassName: "Leadership Lab",
                Start: today.AddDays(4).AddHours(15),
                End: today.AddDays(4).AddHours(17),
                Room: "Studio 205",
                Focus: "Team coaching scenarios",
                IsOnline: false)
        };

        RegistrationQueue = new List<RegistrationRequest>
        {
            new(
                Student: "Leo Ward",
                ClassName: "Modern Web Apps",
                RequestedOn: today.AddDays(-1).AddHours(14),
                Status: "Pending"),
            new(
                Student: "Gabrielle Flores",
                ClassName: "AI Fundamentals",
                RequestedOn: today.AddDays(-2).AddHours(10),
                Status: "Waitlisted"),
            new(
                Student: "Zoe Carter",
                ClassName: "Data Storytelling",
                RequestedOn: today.AddHours(-4),
                Status: "Pending"),
            new(
                Student: "Marcus Flynn",
                ClassName: "Leadership Lab",
                RequestedOn: today.AddDays(-3).AddHours(9),
                Status: "Approved")
        };
    }

    public enum ClassState
    {
        InSession,
        EnrollmentOpen,
        Waitlisted,
        Completed
    }

    public record ClassSummary(
        string Name,
        string Code,
        string Subject,
        string Instructor,
        string Room,
        int Enrollment,
        int Capacity,
        double AttendanceRate,
        DateTime NextSession,
        ClassState State,
        DateTime LastUpdated)
    {
        public string StatusLabel => State switch
        {
            ClassState.InSession => "In session",
            ClassState.EnrollmentOpen => "Enrollment open",
            ClassState.Waitlisted => "Waitlisted",
            ClassState.Completed => "Completed",
            _ => "Scheduled"
        };

        public string StatusBadge => State switch
        {
            ClassState.InSession => "bg-emerald-100 text-emerald-700",
            ClassState.EnrollmentOpen => "bg-sky-100 text-sky-700",
            ClassState.Waitlisted => "bg-amber-100 text-amber-700",
            ClassState.Completed => "bg-slate-200 text-slate-700",
            _ => "bg-slate-100 text-slate-700"
        };
    }

    public record ScheduleItem(
        string ClassName,
        DateTime Start,
        DateTime End,
        string Room,
        string Focus,
        bool IsOnline)
    {
        public string DeliveryBadge => IsOnline ? "Online" : "On-site";

        public string DeliveryStyles => IsOnline ? "bg-purple-100 text-purple-700" : "bg-blue-100 text-blue-700";
    }

    public record RegistrationRequest(
        string Student,
        string ClassName,
        DateTime RequestedOn,
        string Status)
    {
        public string StatusBadge => Status.Equals("Approved", StringComparison.OrdinalIgnoreCase)
            ? "bg-emerald-100 text-emerald-700"
            : Status.Equals("Waitlisted", StringComparison.OrdinalIgnoreCase)
                ? "bg-amber-100 text-amber-700"
                : "bg-slate-100 text-slate-700";
    }
}

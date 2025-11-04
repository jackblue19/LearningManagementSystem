using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Areas.Teachers.Pages;

public class ScheduleModel : PageModel
{
    [BindProperty]
    public ScheduleInput Input { get; set; } = new();

    public List<SelectListItem> ClassOptions { get; private set; } = new();

    public List<SelectListItem> DeliveryModes { get; private set; } = new();

    public IReadOnlyList<TeacherScheduleEntry> ScheduleEntries { get; private set; } = Array.Empty<TeacherScheduleEntry>();

    public ScheduleConfirmation? Confirmation { get; private set; }

    public void OnGet()
    {
        LoadReferenceData();
        LoadSchedule();

        Input.Date ??= DateTime.Today.AddDays(1);
        Input.StartTime ??= new TimeSpan(9, 0, 0);
        Input.EndTime ??= new TimeSpan(10, 30, 0);
    }

    public void OnPost()
    {
        LoadReferenceData();
        LoadSchedule();

        if (!ModelState.IsValid)
        {
            return;
        }

        var selectedClass = ClassOptions.FirstOrDefault(c => c.Value == Input.ClassCode)?.Text ?? Input.ClassCode ?? "Class";
        var deliveryLabel = DeliveryModes.FirstOrDefault(d => d.Value == Input.DeliveryMode)?.Text ?? Input.DeliveryMode ?? "Format";

        var start = Input.Date!.Value.Date + Input.StartTime!.Value;
        var end = Input.Date!.Value.Date + Input.EndTime!.Value;

        Confirmation = new ScheduleConfirmation(
            ClassName: selectedClass,
            Start: start,
            End: end,
            DeliveryMode: deliveryLabel,
            Room: string.IsNullOrWhiteSpace(Input.Room) ? "TBD" : Input.Room!,
            Focus: string.IsNullOrWhiteSpace(Input.Focus) ? "Session outline pending" : Input.Focus!,
            SendInvites: Input.SendInvites);

        var newEntry = new TeacherScheduleEntry(
            ClassName: selectedClass,
            Start: start,
            End: end,
            DeliveryMode: deliveryLabel,
            Room: Confirmation.Room,
            Focus: Confirmation.Focus);

        ScheduleEntries = new[] { newEntry }
            .Concat(ScheduleEntries)
            .OrderBy(e => e.Start)
            .Take(8)
            .ToList();

        TempData["Success"] = $"Session scheduled for {selectedClass}.";

        var nextDate = start.Date.AddDays(1);
        var startTime = Input.StartTime!.Value;
        var endTime = Input.EndTime!.Value;
        var deliveryMode = Input.DeliveryMode;

        ModelState.Clear();
        Input = new ScheduleInput
        {
            Date = nextDate,
            StartTime = startTime,
            EndTime = endTime,
            DeliveryMode = deliveryMode,
            SendInvites = true
        };
    }

    private void LoadReferenceData()
    {
        ClassOptions = new List<SelectListItem>
        {
            new("Modern Web Apps (WEB-302)", "WEB-302"),
            new("Data Storytelling (ANA-210)", "ANA-210"),
            new("AI Fundamentals (AI-101)", "AI-101"),
            new("Leadership Lab (LDR-115)", "LDR-115")
        };

        DeliveryModes = new List<SelectListItem>
        {
            new("On-site", "onsite"),
            new("Hybrid", "hybrid"),
            new("Online", "online")
        };

        Input.DeliveryMode ??= DeliveryModes.First().Value;
    }

    private void LoadSchedule()
    {
        var today = DateTime.Today;
        ScheduleEntries = new List<TeacherScheduleEntry>
        {
            new("Modern Web Apps (WEB-302)", today.AddDays(1).AddHours(9), today.AddDays(1).AddHours(11), "On-site", "Innovation Lab 2B", "Sprint retrospectives"),
            new("Data Storytelling (ANA-210)", today.AddDays(2).AddHours(13), today.AddDays(2).AddHours(15), "On-site", "Room 401", "Interactive dashboards"),
            new("AI Fundamentals (AI-101)", today.AddDays(3).AddHours(10), today.AddDays(3).AddHours(12), "Online", "Teams Live", "Model evaluation clinic"),
            new("Leadership Lab (LDR-115)", today.AddDays(4).AddHours(15), today.AddDays(4).AddHours(17), "Hybrid", "Studio 205", "Team coaching scenarios")
        };
    }

    public class ScheduleInput
    {
        [Required]
        [Display(Name = "Class")]
        public string? ClassCode { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date")]
        public DateTime? Date { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Start time")]
        public TimeSpan? StartTime { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "End time")]
        public TimeSpan? EndTime { get; set; }

        [Display(Name = "Focus area")]
        [StringLength(120)]
        public string? Focus { get; set; }

        [Display(Name = "Room / link")]
        [StringLength(80)]
        public string? Room { get; set; }

        [Display(Name = "Delivery mode")]
        public string? DeliveryMode { get; set; }

        [Display(Name = "Send invites")]
        public bool SendInvites { get; set; } = true;
    }

    public record TeacherScheduleEntry(string ClassName, DateTime Start, DateTime End, string DeliveryMode, string Room, string Focus)
    {
        public string DeliveryBadge => DeliveryMode switch
        {
            "Online" => "bg-purple-100 text-purple-700",
            "Hybrid" => "bg-amber-100 text-amber-700",
            _ => "bg-blue-100 text-blue-700"
        };
    }

    public record ScheduleConfirmation(string ClassName, DateTime Start, DateTime End, string DeliveryMode, string Room, string Focus, bool SendInvites);
}

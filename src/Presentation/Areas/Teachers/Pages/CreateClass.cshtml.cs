using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Areas.Teachers.Pages;

public class CreateClassModel : PageModel
{
    [BindProperty]
    public CreateClassInput Input { get; set; } = new();

    public List<SelectListItem> SubjectOptions { get; private set; } = new();

    public List<SelectListItem> InstructorOptions { get; private set; } = new();

    public List<SelectListItem> DeliveryModes { get; private set; } = new();

    public CreatedClassSummary? Summary { get; private set; }

    public void OnGet()
    {
        LoadReferenceData();
        Input.StartDate ??= DateTime.Today.AddDays(7);
        Input.EndDate ??= DateTime.Today.AddMonths(3);
        Input.Capacity = Input.Capacity == 0 ? 24 : Input.Capacity;
    }

    public void OnPost()
    {
        LoadReferenceData();

        if (!ModelState.IsValid)
        {
            return;
        }

        var subjectName = SubjectOptions.FirstOrDefault(s => s.Value == Input.SubjectId)?.Text ?? Input.SubjectId ?? "Subject";
        var instructorName = InstructorOptions.FirstOrDefault(i => i.Value == Input.InstructorId)?.Text ?? Input.InstructorId ?? "Instructor";
        var deliveryLabel = DeliveryModes.FirstOrDefault(m => m.Value == Input.DeliveryMode)?.Text ?? Input.DeliveryMode ?? "Format";

        Summary = new CreatedClassSummary(
            Name: Input.Name!,
            Code: Input.Code!,
            Subject: subjectName,
            Instructor: instructorName,
            StartDate: Input.StartDate!.Value,
            EndDate: Input.EndDate!.Value,
            Capacity: Input.Capacity,
            DeliveryMode: deliveryLabel,
            StartTime: Input.StartTime!.Value,
            EndTime: Input.EndTime!.Value,
            DaysOfWeek: Input.DaysOfWeek ?? string.Empty,
            Room: Input.Room,
            Description: Input.Description);

        TempData["Success"] = $"Draft class {Input.Name} is ready for review.";

        ModelState.Clear();
        Input = new CreateClassInput
        {
            StartDate = DateTime.Today.AddDays(7),
            EndDate = DateTime.Today.AddMonths(3),
            Capacity = 24,
            DaysOfWeek = "Tue & Thu",
            DeliveryMode = DeliveryModes.FirstOrDefault()?.Value,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(11, 0, 0)
        };
    }

    private void LoadReferenceData()
    {
        SubjectOptions = new List<SelectListItem>
        {
            new("Advanced Frontend Engineering", "WEB-302"),
            new("Visual Analytics", "ANA-210"),
            new("Applied Machine Learning", "AI-101"),
            new("Collaborative Leadership", "LDR-115")
        };

        InstructorOptions = new List<SelectListItem>
        {
            new("Amelia Benson", "teacher-001"),
            new("Mason Ruiz", "teacher-002"),
            new("Priya Nair", "teacher-003"),
            new("Noah Adams", "teacher-004")
        };

        DeliveryModes = new List<SelectListItem>
        {
            new("On-site", "onsite"),
            new("Hybrid", "hybrid"),
            new("Online", "online")
        };

        Input.DeliveryMode ??= DeliveryModes.First().Value;
    }

    public class CreateClassInput
    {
        [Required]
        [Display(Name = "Class name")]
        [StringLength(80)]
        public string? Name { get; set; }

        [Required]
        [Display(Name = "Code")]
        [StringLength(20)]
        public string? Code { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public string? SubjectId { get; set; }

        [Required]
        [Display(Name = "Instructor")]
        public string? InstructorId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime? StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End date")]
        public DateTime? EndDate { get; set; }

        [Range(1, 60)]
        [Display(Name = "Capacity")]
        public int Capacity { get; set; }

        [Required]
        [Display(Name = "Delivery mode")]
        public string? DeliveryMode { get; set; }

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Starts at")]
        public TimeSpan? StartTime { get; set; } = new TimeSpan(9, 0, 0);

        [Required]
        [DataType(DataType.Time)]
        [Display(Name = "Ends at")]
        public TimeSpan? EndTime { get; set; } = new TimeSpan(11, 0, 0);

        [Display(Name = "Delivery days")]
        [StringLength(80)]
        public string? DaysOfWeek { get; set; } = "Mon & Wed";

        [Display(Name = "Room / meeting link")]
        [StringLength(80)]
        public string? Room { get; set; }

        [Display(Name = "Overview")]
        [StringLength(400)]
        public string? Description { get; set; }
    }

    public record CreatedClassSummary(
        string Name,
        string Code,
        string Subject,
        string Instructor,
        DateTime StartDate,
        DateTime EndDate,
        int Capacity,
        string DeliveryMode,
        TimeSpan StartTime,
        TimeSpan EndTime,
        string DaysOfWeek,
        string? Room,
        string? Description);
}

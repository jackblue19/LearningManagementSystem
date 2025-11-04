using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Areas.Teachers.Pages;

public class RegisterClassModel : PageModel
{
    private static readonly IReadOnlyList<SelectListItem> SeedClasses = new List<SelectListItem>
    {
        new("Modern Web Apps (WEB-302)", "WEB-302"),
        new("Data Storytelling (ANA-210)", "ANA-210"),
        new("AI Fundamentals (AI-101)", "AI-101"),
        new("Leadership Lab (LDR-115)", "LDR-115")
    };

    private static readonly IReadOnlyList<SelectListItem> SeedStudents = new List<SelectListItem>
    {
        new("Amelia Vega", "stu-001"),
        new("Leo Ward", "stu-002"),
        new("Gabrielle Flores", "stu-003"),
        new("Marcus Flynn", "stu-004"),
        new("Zoe Carter", "stu-005")
    };

    [BindProperty]
    public RegistrationInput Input { get; set; } = new();

    public List<SelectListItem> ClassOptions { get; private set; } = new();

    public List<SelectListItem> StudentOptions { get; private set; } = new();

    public IReadOnlyList<RecentRegistration> RecentRegistrations { get; private set; } = Array.Empty<RecentRegistration>();

    public RegistrationReceipt? Receipt { get; private set; }

    public void OnGet()
    {
        LoadReferenceData();
        Input.StartDate ??= DateTime.Today.AddDays(2);
    }

    public void OnPost()
    {
        LoadReferenceData();

        if (!ModelState.IsValid)
        {
            return;
        }

        var selectedClass = ClassOptions.FirstOrDefault(c => c.Value == Input.ClassId)?.Text ?? "Selected class";
        var selectedStudent = StudentOptions.FirstOrDefault(s => s.Value == Input.StudentId)?.Text ?? "Selected student";

        Receipt = new RegistrationReceipt(
            Student: selectedStudent,
            ClassName: selectedClass,
            StartDate: Input.StartDate!.Value,
            SeatsReserved: Input.Seats,
            Notes: Input.Notes);

        var newEntry = new RecentRegistration(
            Student: selectedStudent,
            ClassName: selectedClass,
            RequestedOn: DateTime.UtcNow,
            Status: "Approved");

        RecentRegistrations = new[] { newEntry }
            .Concat(RecentRegistrations)
            .Take(6)
            .ToList();

        TempData["Success"] = $"{selectedStudent} has been registered to {selectedClass}.";

        ModelState.Clear();
        Input = new RegistrationInput
        {
            StartDate = DateTime.Today.AddDays(2),
            Seats = 1
        };
    }

    private void LoadReferenceData()
    {
        ClassOptions = SeedClasses.ToList();
        StudentOptions = SeedStudents.ToList();
        RecentRegistrations = new List<RecentRegistration>
        {
            new("Leo Ward", "Modern Web Apps (WEB-302)", DateTime.UtcNow.AddHours(-5), "Pending"),
            new("Gabrielle Flores", "AI Fundamentals (AI-101)", DateTime.UtcNow.AddHours(-12), "Waitlisted"),
            new("Marcus Flynn", "Leadership Lab (LDR-115)", DateTime.UtcNow.AddDays(-1), "Approved"),
            new("Priya Patel", "Data Storytelling (ANA-210)", DateTime.UtcNow.AddDays(-2), "Approved")
        };
    }

    public class RegistrationInput
    {
        [Required]
        [Display(Name = "Class")]
        public string? ClassId { get; set; }

        [Required]
        [Display(Name = "Learner")]
        public string? StudentId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start date")]
        public DateTime? StartDate { get; set; }

        [Range(1, 10)]
        [Display(Name = "Seats")]
        public int Seats { get; set; } = 1;

        [Display(Name = "Notes")]
        [StringLength(250)]
        public string? Notes { get; set; }
    }

    public record RecentRegistration(string Student, string ClassName, DateTime RequestedOn, string Status)
    {
        public string StatusBadge => Status switch
        {
            "Approved" => "bg-emerald-100 text-emerald-700",
            "Waitlisted" => "bg-amber-100 text-amber-700",
            _ => "bg-slate-100 text-slate-700"
        };
    }

    public record RegistrationReceipt(string Student, string ClassName, DateTime StartDate, int SeatsReserved, string? Notes);
}

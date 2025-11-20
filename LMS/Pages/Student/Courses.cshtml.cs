using System.ComponentModel.DataAnnotations;
using LMS.Models.Entities;
using LMS.Services.Interfaces.StudentService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Student;

public class CoursesModel : PageModel
{
    private readonly IStudentCourseService _courseService;

    public CoursesModel(IStudentCourseService courseService)
    {
        _courseService = courseService;
    }

    // -------------------------------
    // Query & Form properties
    // -------------------------------
    [BindProperty(SupportsGet = true)]
    [Display(Name = "Student Id")]
    public Guid StudentId { get; set; }

    [BindProperty(SupportsGet = true)]
    [Display(Name = "Show cancelled")]
    public bool IncludeCancelled { get; set; }

    // -------------------------------
    // View data
    // -------------------------------
    public IReadOnlyList<Class> Courses { get; private set; } = Array.Empty<Class>();

    // -------------------------------
    // GET: load courses if StudentId provided
    // -------------------------------
    public async Task OnGetAsync(CancellationToken ct)
    {
        if (StudentId == Guid.Empty)
            return;

        Courses = await _courseService.GetRegisteredClassesAsync(StudentId, IncludeCancelled, ct);
    }

    // -------------------------------
    // POST: triggered by Load Courses button
    // -------------------------------
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (StudentId == Guid.Empty)
        {
            ModelState.AddModelError(nameof(StudentId), "Student Id is required.");
            return Page();
        }

        Courses = await _courseService.GetRegisteredClassesAsync(StudentId, IncludeCancelled, ct);

        // Return the page (not redirect) so that validation messages appear
        return Page();
    }
}

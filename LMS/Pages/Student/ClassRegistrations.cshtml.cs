using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Services.Interfaces.StudentService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Student;

public class ClassRegistrationsModel : PageModel
{
    private readonly IClassRegistrationService _registrationService;
    private readonly IClassRepository _classRepository;
    private readonly IClassRegistrationRepository _registrationRepository;

    public ClassRegistrationsModel(
        IClassRegistrationService registrationService,
        IClassRepository classRepository,
        IClassRegistrationRepository registrationRepository)
    {
        _registrationService = registrationService;
        _classRepository = classRepository;
        _registrationRepository = registrationRepository;
    }

    // -------------------------
    // Query + Form Properties
    // -------------------------
    [BindProperty(SupportsGet = true)]
    [Display(Name = "Student Id")]
    public Guid StudentId { get; set; }

    [BindProperty]
    [Display(Name = "Class Id")]
    public Guid ClassId { get; set; }

    // -------------------------
    // View Data
    // -------------------------
    public IReadOnlyList<Class> Classes { get; private set; } = Array.Empty<Class>();
    public HashSet<Guid> RegisteredClassIds { get; private set; } = new();

    // -------------------------
    // UI Info About Registration
    // -------------------------
    public bool? IsRegistered { get; set; }

    // -------------------------
    // System messages
    // -------------------------
    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    // --------------------------------------------------------
    // GET – Show form + check current registration state
    // --------------------------------------------------------
    public async Task OnGetAsync(CancellationToken ct)
    {
        if (StudentId == Guid.Empty)
            return;

        // Load all classes with Teacher navigation
        var classIncludes = new Expression<Func<Class, object>>[]
        {
            c => c.Teacher
        };

        Classes = await _classRepository.ListAsync(
            predicate: null,
            orderBy: q => q.OrderBy(c => c.ClassName),
            includes: classIncludes,
            ct: ct);

        // Load registered class IDs for this student (active registrations only)
        var registrations = await _registrationRepository.ListAsync(
            predicate: r => r.StudentId == StudentId &&
                           string.Equals(r.RegistrationStatus, "Active", StringComparison.OrdinalIgnoreCase),
            ct: ct);

        RegisteredClassIds = registrations
            .Select(r => r.ClassId)
            .ToHashSet();

        // Check specific class registration status if ClassId is provided
        if (ClassId != Guid.Empty)
        {
            IsRegistered = await _registrationService.IsRegisteredAsync(StudentId, ClassId, ct);
        }
    }

    // --------------------------------------------------------
    // POST – Register
    // --------------------------------------------------------
    public async Task<IActionResult> OnPostRegisterAsync(CancellationToken ct)
    {
        if (StudentId == Guid.Empty || ClassId == Guid.Empty)
        {
            ErrorMessage = "Student Id and Class Id are required.";
            return RedirectToPage(new { studentId = StudentId });
        }

        var success = await _registrationService.RegisterAsync(StudentId, ClassId, ct);

        if (success)
        {
            StatusMessage = "Registration created / reactivated successfully.";
        }
        else
        {
            ErrorMessage = "Failed to register. The student may already be registered, or data is invalid.";
        }

        return RedirectToPage(new { studentId = StudentId });
    }

    // --------------------------------------------------------
    // POST – Cancel
    // --------------------------------------------------------
    public async Task<IActionResult> OnPostCancelAsync(CancellationToken ct)
    {
        if (StudentId == Guid.Empty || ClassId == Guid.Empty)
        {
            ErrorMessage = "Student Id and Class Id are required.";
            return RedirectToPage(new { studentId = StudentId });
        }

        var success = await _registrationService.CancelAsync(StudentId, ClassId, ct);

        if (success)
        {
            StatusMessage = "Registration cancelled successfully.";
        }
        else
        {
            ErrorMessage = "Failed to cancel. The student may not be registered or is already cancelled.";
        }

        return RedirectToPage(new { studentId = StudentId });
    }

}

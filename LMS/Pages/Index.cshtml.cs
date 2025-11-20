using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        // If user is authenticated, redirect to appropriate dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            return role?.ToLower() switch
            {
                "admin" => RedirectToPage("/Admin/AdminDashboard"),
                "manager" => RedirectToPage("/Manager/ManagerDashboard"),
                "teacher" => RedirectToPage("/Teacher/TeacherDashboard"),
                "student" => RedirectToPage("/Student/StudentDashboard"),
                _ => RedirectToPage("/Student/StudentDashboard")
            };
        }

        // If not authenticated, redirect to login page
        return RedirectToPage("/Common/Login");
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Presentation.Areas.Shared.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var username = Request.Form["Username"].ToString();
            var role = Request.Form["Role"].ToString();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
            {
                ModelState.AddModelError(string.Empty, "Username and role are required.");
                return Page();
            }

            // NOTE: This is a lightweight demo registration flow.
            // In production, persist the user via the Application layer and enforce validation.
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync("Cookies", principal, authProperties);

            string area = role switch
            {
                "Student" => "Students",
                "Teacher" => "Teachers",
                "Admin" => "Admin",
                "CentreManagement" => "CentreManagement",
                _ => ""
            };

            if (!string.IsNullOrEmpty(area))
                return Redirect($"/{area}/Dashboard");

            return RedirectToPage("/Index", new { area = "" });
        }
    }
}

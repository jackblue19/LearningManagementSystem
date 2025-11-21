using LMS.Models.Entities;
using LMS.Models.ViewModels.Auth;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LMS.Pages.Common;

public class LoginModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly LMS.Services.Interfaces.AdminService.IAuditLogService _auditLogService;

    public LoginModel(IAuthService authService, LMS.Services.Interfaces.AdminService.IAuditLogService auditLogService)
    {
        _authService = authService;
        _auditLogService = auditLogService;
    }

    [BindProperty]
    public LoginViewModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        
        // Get messages from TempData
        if (TempData["ErrorMessage"] != null)
        {
            ErrorMessage = TempData["ErrorMessage"]?.ToString();
        }
        if (TempData["SuccessMessage"] != null)
        {
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
        }
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Vui lòng nhập đầy đủ thông tin.";
            return Page();
        }

        var (success, user, errorMessage) = await _authService.LoginAsync(
            Input.UsernameOrEmail, 
            Input.Password);

        if (!success || user == null)
        {
            ErrorMessage = errorMessage ?? "Đăng nhập thất bại.";
            return Page();
        }

        // Create claims
        await SignInUserAsync(user, Input.RememberMe);

        // Log Audit
        await _auditLogService.LogActionAsync(
            userId: user.UserId,
            actionType: "Login",
            entityName: "User",
            recordId: user.UserId.ToString(),
            newData: $"{{ \"IP\": \"{HttpContext.Connection.RemoteIpAddress}\" }}"
        );

        // Redirect to appropriate dashboard

        // Redirect to appropriate dashboard
        return RedirectToRoleDashboard(user.RoleDesc, returnUrl);
    }

    public IActionResult OnGetGoogleLogin(string? returnUrl = null)
    {
        Console.WriteLine("=== OnGetGoogleLogin called ===");
        Console.WriteLine($"ReturnUrl: {returnUrl}");
        
        var redirectUri = Url.Page("/Common/Login", pageHandler: "GoogleCallback", values: new { returnUrl });
        Console.WriteLine($"RedirectUri: {redirectUri}");
        
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUri,
            Items = { { "scheme", GoogleDefaults.AuthenticationScheme } }
        };
        
        Console.WriteLine("Initiating Google Challenge...");
        var result = Challenge(properties, GoogleDefaults.AuthenticationScheme);
        Console.WriteLine($"Challenge result type: {result.GetType().Name}");
        return result;
    }

    public async Task<IActionResult> OnGetGoogleCallbackAsync(string? returnUrl = null)
    {
        try
        {
            Console.WriteLine("=== Google Callback Started ===");
            
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            Console.WriteLine($"Authenticate Succeeded: {authenticateResult.Succeeded}");

            if (!authenticateResult.Succeeded)
            {
                Console.WriteLine($"Auth failed: {authenticateResult.Failure?.Message}");
                TempData["ErrorMessage"] = "Đăng nhập Google thất bại.";
                return RedirectToPage("/Common/Login", new { returnUrl });
            }

            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);
            var picture = authenticateResult.Principal.FindFirstValue("picture");
            
            Console.WriteLine($"Email: {email}, Name: {name}");

            if (string.IsNullOrEmpty(email))
            {
                TempData["ErrorMessage"] = "Không lấy được thông tin email từ Google.";
                return RedirectToPage("/Common/Login", new { returnUrl });
            }

            var (success, user, errorMessage, isNewUser) = await _authService.GoogleLoginAsync(email, name ?? email, picture);

            Console.WriteLine($"GoogleLoginAsync - Success: {success}, IsNewUser: {isNewUser}, User: {user?.Email}");

            if (!success || user == null)
            {
                Console.WriteLine($"Login failed: {errorMessage}");
                TempData["ErrorMessage"] = errorMessage ?? "Đăng nhập Google thất bại.";
                return RedirectToPage("/Common/Login", new { returnUrl });
            }

            // Sign in with Cookie authentication (this will override Google auth)
            await SignInUserAsync(user, true);
            
            Console.WriteLine($"SignIn completed for user: {user.Email}, UserId: {user.UserId}, Role: {user.RoleDesc}");

            // If new Google user, redirect to setup password page
            if (isNewUser)
            {
                Console.WriteLine("Redirecting to SetupPassword");
                TempData["InfoMessage"] = "Vui lòng thiết lập mật khẩu cho tài khoản của bạn.";
                return RedirectToPage("/Common/SetupPassword");
            }

            // Redirect to role-based dashboard
            Console.WriteLine($"Redirecting to dashboard for role: {user.RoleDesc}");
            return RedirectToRoleDashboard(user.RoleDesc, returnUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GoogleCallback: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            TempData["ErrorMessage"] = $"Lỗi xử lý đăng nhập Google: {ex.Message}";
            return RedirectToPage("/Common/Login", new { returnUrl });
        }
    }

    private async Task SignInUserAsync(User user, bool rememberMe)
    {
        Console.WriteLine($"[SignInUserAsync] Creating claims for UserId: {user.UserId}");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.RoleDesc ?? "student"),
            new Claim("FullName", user.FullName ?? user.Username),
            new Claim("Avatar", user.Avatar ?? "/images/default-avatar.png")
        };

        Console.WriteLine($"[SignInUserAsync] Claims created - NameIdentifier: {user.UserId}, Role: {user.RoleDesc}");

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(12)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
            
        Console.WriteLine($"[SignInUserAsync] SignIn completed successfully");
    }

    private IActionResult RedirectToRoleDashboard(string? role, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return role?.ToLower() switch
        {
            "admin" => RedirectToPage("/Admin/AdminDashboard"),
            "manager" => RedirectToPage("/Manager/ManagerDashboard"),
            "teacher" => RedirectToPage("/Teacher/TeacherDashboard"),
            "student" => RedirectToPage("/Student/StudentDashboard"),
            _ => RedirectToPage("/Student/StudentDashboard")
        };
    }
}

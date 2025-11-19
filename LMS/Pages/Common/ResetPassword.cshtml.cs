using LMS.Models.ViewModels.Auth;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace LMS.Pages.Common;

public class ResetPasswordModel : PageModel
{
    private readonly IAuthService _authService;
    private readonly IMemoryCache _cache;

    public ResetPasswordModel(IAuthService authService, IMemoryCache cache)
    {
        _authService = authService;
        _cache = cache;
    }

    [BindProperty]
    public ResetPasswordViewModel Model { get; set; } = new();

    [BindProperty]
    public string Token { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    public string? ErrorMessage { get; set; }
    public bool IsSuccess { get; set; }

    public IActionResult OnGet(string token, string email)
    {
        Console.WriteLine($"[ResetPassword] OnGet called - Token: {token}, Email: {email}");
        
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            Console.WriteLine("[ResetPassword] Token or email is empty");
            ErrorMessage = "Link đặt lại mật khẩu không hợp lệ.";
            return Page();
        }

        // URL decode the email (in case it was encoded in the link)
        var decodedEmail = Uri.UnescapeDataString(email);
        Console.WriteLine($"[ResetPassword] Decoded email: {decodedEmail}");

        // Validate token from cache
        var cacheKey = $"reset_token_{decodedEmail}";
        Console.WriteLine($"[ResetPassword] Looking for cache key: {cacheKey}");
        
        if (!_cache.TryGetValue(cacheKey, out string? cachedToken))
        {
            Console.WriteLine("[ResetPassword] Token not found in cache - may have expired or server was restarted");
            ErrorMessage = "Link đặt lại mật khẩu đã hết hạn hoặc không hợp lệ. Vui lòng yêu cầu link mới.";
            return Page();
        }
        
        Console.WriteLine($"[ResetPassword] Cached token: {cachedToken}");
        
        if (cachedToken != token)
        {
            Console.WriteLine("[ResetPassword] Token mismatch");
            ErrorMessage = "Link đặt lại mật khẩu đã hết hạn hoặc không hợp lệ.";
            return Page();
        }

        Console.WriteLine("[ResetPassword] Token validation successful");
        Token = token;
        Email = decodedEmail;
        Model.Token = token;
        Model.Email = decodedEmail;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Console.WriteLine($"[ResetPassword] OnPost called - Email: {Model.Email}, Token: {Model.Token}");
        
        if (!ModelState.IsValid)
        {
            Console.WriteLine("[ResetPassword] ModelState invalid");
            return Page();
        }

        // Validate token again
        var cacheKey = $"reset_token_{Model.Email}";
        Console.WriteLine($"[ResetPassword] Validating cache key: {cacheKey}");
        
        if (!_cache.TryGetValue(cacheKey, out string? cachedToken))
        {
            Console.WriteLine("[ResetPassword] Token not found in cache during post");
            ErrorMessage = "Link đặt lại mật khẩu đã hết hạn hoặc không hợp lệ.";
            return Page();
        }
        
        if (cachedToken != Model.Token)
        {
            Console.WriteLine("[ResetPassword] Token mismatch during post");
            ErrorMessage = "Link đặt lại mật khẩu đã hết hạn hoặc không hợp lệ.";
            return Page();
        }

        // Reset password
        Console.WriteLine($"[ResetPassword] Resetting password for: {Model.Email}");
        var result = await _authService.ResetPasswordAsync(Model.Email, Model.Password);
        if (!result)
        {
            Console.WriteLine("[ResetPassword] Password reset failed");
            ErrorMessage = "Không thể đặt lại mật khẩu. Vui lòng thử lại.";
            return Page();
        }

        // Remove token from cache after successful reset
        _cache.Remove(cacheKey);
        Console.WriteLine("[ResetPassword] Password reset successful, token removed");

        IsSuccess = true;
        return Page();
    }
}

using LMS.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Common;

public class TestEmailModel : PageModel
{
    private readonly EmailHelper _emailHelper;
    private readonly IConfiguration _configuration;

    public TestEmailModel(EmailHelper emailHelper, IConfiguration configuration)
    {
        _emailHelper = emailHelper;
        _configuration = configuration;
    }

    [BindProperty]
    public string TestEmail { get; set; } = "";

    public string? Message { get; set; }
    public bool IsSuccess { get; set; }

    // Display config
    public string SmtpHost => _configuration["Email:SmtpHost"] ?? "N/A";
    public string SmtpPort => _configuration["Email:SmtpPort"] ?? "N/A";
    public string FromEmail => _configuration["Email:FromEmail"] ?? "N/A";
    public string FromPassword => _configuration["Email:FromPassword"] ?? "";
    public string FromName => _configuration["Email:FromName"] ?? "N/A";

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(TestEmail))
        {
            Message = "Vui lòng nhập email!";
            IsSuccess = false;
            return Page();
        }

        Console.WriteLine($"[TestEmail] Attempting to send test email to: {TestEmail}");
        Console.WriteLine($"[TestEmail] SMTP Config: {SmtpHost}:{SmtpPort}");
        Console.WriteLine($"[TestEmail] From: {FromEmail}");
        Console.WriteLine($"[TestEmail] Password configured: {!string.IsNullOrEmpty(FromPassword)}");

        var resetLink = "https://localhost:7045/Common/ResetPassword?token=test123&email=test@example.com";
        
        var result = await _emailHelper.SendPasswordResetEmailAsync(TestEmail, resetLink);

        if (result)
        {
            Message = $"✅ Email đã được gửi thành công đến {TestEmail}! Vui lòng kiểm tra hộp thư (và cả spam folder).";
            IsSuccess = true;
        }
        else
        {
            Message = "❌ Không thể gửi email. Vui lòng kiểm tra:\n" +
                     "1. Gmail App Password có đúng không?\n" +
                     "2. Gmail có bật 2-Step Verification không?\n" +
                     "3. App Password có bị revoke không?\n" +
                     "Xem console log để biết thêm chi tiết.";
            IsSuccess = false;
        }

        return Page();
    }
}

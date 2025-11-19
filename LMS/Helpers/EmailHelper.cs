using System.Net;
using System.Net.Mail;

namespace LMS.Helpers;

public class EmailHelper
{
    private readonly IConfiguration _configuration;

    public EmailHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Send email using Gmail SMTP
    /// </summary>
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var fromEmail = _configuration["Email:FromEmail"];
            var fromPassword = _configuration["Email:FromPassword"];
            var fromName = _configuration["Email:FromName"] ?? "LMS System";

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassword))
            {
                Console.WriteLine("[EmailHelper] Email configuration not found");
                return false;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(fromEmail, fromPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"[EmailHelper] Email sent successfully to {toEmail}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailHelper] Failed to send email: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Send password reset email
    /// </summary>
    public async Task<bool> SendPasswordResetEmailAsync(string toEmail, string resetLink)
    {
        var subject = "Đặt lại mật khẩu - LMS";
        var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border-radius: 10px;'>
                    <div style='background: white; padding: 30px; border-radius: 10px;'>
                        <h2 style='color: #667eea; text-align: center;'>Đặt lại mật khẩu</h2>
                        <p>Xin chào,</p>
                        <p>Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
                        <p>Vui lòng click vào nút bên dưới để đặt lại mật khẩu:</p>
                        <div style='text-align: center; margin: 30px 0;'>
                            <a href='{resetLink}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 15px 30px; text-decoration: none; border-radius: 10px; display: inline-block; font-weight: bold;'>
                                Đặt lại mật khẩu
                            </a>
                        </div>
                        <p style='color: #666;'>Link này sẽ hết hạn sau 30 phút.</p>
                        <p style='color: #666;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;'>
                        <p style='color: #999; font-size: 12px; text-align: center;'>
                            © 2025 Learning Management System. All rights reserved.
                        </p>
                    </div>
                </div>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, subject, body);
    }
}

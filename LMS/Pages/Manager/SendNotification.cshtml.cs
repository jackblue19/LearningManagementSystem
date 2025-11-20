using LMS.Models.ViewModels.Notification;
using LMS.Services.Interfaces.ManagerService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CommonNotificationService = LMS.Services.Interfaces.CommonService.INotificationService;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class SendNotificationModel : PageModel
{
    private readonly CommonNotificationService _notificationService;
    private readonly IClassService _classService;

    public SendNotificationModel(
        CommonNotificationService notificationService,
        IClassService classService)
    {
        _notificationService = notificationService;
        _classService = classService;
    }

    [BindProperty]
    public CreateNotificationViewModel Input { get; set; } = new();

    public List<SelectListItem> Classes { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        await LoadClassesAsync();

        if (TempData["SuccessMessage"] != null)
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
        if (TempData["ErrorMessage"] != null)
            ErrorMessage = TempData["ErrorMessage"]?.ToString();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Generate unique request ID for logging
        var requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
        Console.WriteLine($"[SendNotification-{requestId}] POST request started at {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        
        if (!ModelState.IsValid)
        {
            Console.WriteLine($"[SendNotification-{requestId}] ModelState invalid");
            await LoadClassesAsync();
            return Page();
        }

        try
        {
            var sentCount = 0;
            Console.WriteLine($"[SendNotification-{requestId}] RecipientType: {Input.RecipientType}, Title: {Input.Title}");

            switch (Input.RecipientType)
            {
                case "all-teachers":
                    Console.WriteLine($"[SendNotification-{requestId}] Sending to all teachers...");
                    var teacherNotifications = await _notificationService.SendToAllTeachersAsync(
                        Input.Title, Input.Message, Input.Type);
                    sentCount = teacherNotifications.Count;
                    Console.WriteLine($"[SendNotification-{requestId}] Sent {sentCount} teacher notifications");
                    break;

                case "all-students":
                    Console.WriteLine($"[SendNotification-{requestId}] Sending to all students...");
                    var studentNotifications = await _notificationService.SendToAllStudentsAsync(
                        Input.Title, Input.Message, Input.Type);
                    sentCount = studentNotifications.Count;
                    Console.WriteLine($"[SendNotification-{requestId}] Sent {sentCount} student notifications");
                    break;

                case "class":
                    if (Input.ClassId == null)
                    {
                        Console.WriteLine($"[SendNotification-{requestId}] ClassId is null");
                        ModelState.AddModelError("", "Vui lòng chọn lớp học");
                        await LoadClassesAsync();
                        return Page();
                    }
                    Console.WriteLine($"[SendNotification-{requestId}] Sending to class {Input.ClassId}...");
                    var classNotifications = await _notificationService.SendToClassAsync(
                        Input.ClassId.Value, Input.Title, Input.Message, Input.Type);
                    sentCount = classNotifications.Count;
                    Console.WriteLine($"[SendNotification-{requestId}] Sent {sentCount} class notifications");
                    break;

                case "specific-users":
                    if (Input.UserIds == null || !Input.UserIds.Any())
                    {
                        Console.WriteLine($"[SendNotification-{requestId}] UserIds is empty");
                        ModelState.AddModelError("", "Vui lòng chọn người nhận");
                        await LoadClassesAsync();
                        return Page();
                    }
                    Console.WriteLine($"[SendNotification-{requestId}] Sending to {Input.UserIds.Count} specific users...");
                    var userNotifications = await _notificationService.SendBulkNotificationAsync(
                        Input.UserIds, Input.Title, Input.Message, Input.Type);
                    sentCount = userNotifications.Count;
                    Console.WriteLine($"[SendNotification-{requestId}] Sent {sentCount} user notifications");
                    break;
            }

            TempData["SuccessMessage"] = $"Đã gửi thông báo thành công đến {sentCount} người!";
            Console.WriteLine($"[SendNotification-{requestId}] POST request completed successfully");
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SendNotification-{requestId}] ERROR: {ex.Message}");
            Console.WriteLine($"[SendNotification-{requestId}] StackTrace: {ex.StackTrace}");
            ErrorMessage = $"Lỗi khi gửi thông báo: {ex.Message}";
            await LoadClassesAsync();
            return Page();
        }
    }

    private async Task LoadClassesAsync()
    {
        var classes = await _classService.GetAllClassesAsync();
        Classes = classes.Select(c => new SelectListItem
        {
            Value = c.ClassId.ToString(),
            Text = $"{c.ClassName} - {c.Subject?.SubjectName}"
        }).ToList();
    }
}

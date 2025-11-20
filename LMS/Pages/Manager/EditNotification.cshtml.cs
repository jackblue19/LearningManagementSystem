using LMS.Models.ViewModels.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class EditNotificationModel : PageModel
{
    private readonly Services.Interfaces.CommonService.INotificationService _notificationService;

    public EditNotificationModel(Services.Interfaces.CommonService.INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [BindProperty]
    public EditNotificationViewModel Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy thông báo";
            return RedirectToPage("/Manager/ManageNotifications");
        }

        var contentLines = notification.Content?.Split('\n', 2) ?? new[] { "", "" };
        Input = new EditNotificationViewModel
        {
            NotificationId = notification.NotificationId,
            Title = contentLines.Length > 0 ? contentLines[0] : "",
            Message = contentLines.Length > 1 ? contentLines[1] : "",
            Type = notification.NotiType ?? "info"
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var success = await _notificationService.UpdateNotificationAsync(
                Input.NotificationId,
                Input.Title,
                Input.Message,
                Input.Type);

            if (success)
            {
                TempData["SuccessMessage"] = "Cập nhật thông báo thành công!";
                return RedirectToPage("/Manager/ManageNotifications");
            }
            else
            {
                ErrorMessage = "Không tìm thấy thông báo để cập nhật";
                return Page();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Lỗi khi cập nhật thông báo: {ex.Message}";
            return Page();
        }
    }
}

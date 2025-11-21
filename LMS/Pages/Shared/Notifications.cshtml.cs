using LMS.Models.Entities;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LMS.Pages.Shared;

[Authorize]
public class NotificationsModel : PageModel
{
    private readonly INotificationService _notificationService;

    public NotificationsModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public List<Notification> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool HasMore { get; set; }

    public async Task OnGetAsync(int pageNumber = 1)
    {
        PageNumber = pageNumber;

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return;
        }

        Notifications = await _notificationService.GetUserNotificationsAsync(
            userId, PageNumber, PageSize);
        UnreadCount = await _notificationService.GetUnreadCountAsync(userId);
        HasMore = Notifications.Count == PageSize;
    }

    public async Task<IActionResult> OnPostMarkAsReadAsync(long notificationId)
    {
        await _notificationService.MarkAsReadAsync(notificationId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMarkAllAsReadAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage();
        }

        await _notificationService.MarkAllAsReadAsync(userId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(long notificationId)
    {
        await _notificationService.DeleteNotificationAsync(notificationId);
        return RedirectToPage();
    }
}

using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LMS.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentNotifications()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var notifications = await _notificationService.GetUserNotificationsAsync(userId, 1, 5);
        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);

        return Ok(new
        {
            notifications = notifications.Select(n => new
            {
                notificationId = n.NotificationId,
                content = n.Content,
                notiType = n.NotiType,
                isRead = n.IsRead,
                createdAt = n.CreatedAt
            }),
            unreadCount
        });
    }

    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkAsRead([FromBody] MarkAsReadRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return Unauthorized();
        }

        var success = await _notificationService.MarkAsReadAsync(request.NotificationId);
        
        if (success)
        {
            return Ok(new { success = true });
        }

        return BadRequest(new { success = false, message = "Failed to mark notification as read" });
    }
}

public class MarkAsReadRequest
{
    public long NotificationId { get; set; }
}

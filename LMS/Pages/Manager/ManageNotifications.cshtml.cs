using LMS.Models.Entities;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Manager;

[Authorize(Policy = "ManagerOnly")]
public class ManageNotificationsModel : PageModel
{
    private readonly Services.Interfaces.CommonService.INotificationService _notificationService;

    public ManageNotificationsModel(Services.Interfaces.CommonService.INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public List<NotificationSummary> Notifications { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync(int pageNumber = 1)
    {
        PageNumber = pageNumber;
        
        if (TempData["SuccessMessage"] != null)
            SuccessMessage = TempData["SuccessMessage"]?.ToString();
        if (TempData["ErrorMessage"] != null)
            ErrorMessage = TempData["ErrorMessage"]?.ToString();
        
        // Get all notifications sent by this manager
        var allNotifications = await _notificationService.GetAllNotificationsAsync();
        
        // Count unique notification groups (not individual notifications)
        var uniqueGroups = allNotifications
            .GroupBy(n => new { n.Content, n.NotiType, n.CreatedAt })
            .Count();
        
        TotalCount = uniqueGroups;
        TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
        
        // Group notifications by content to show as single item
        Notifications = allNotifications
            .GroupBy(n => new { n.Content, n.NotiType, n.CreatedAt })
            .Select(g => new NotificationSummary
            {
                NotificationId = g.First().NotificationId, // Use first notification's ID as representative
                Content = g.Key.Content,
                Type = g.Key.NotiType,
                CreatedAt = g.Key.CreatedAt,
                RecipientCount = g.Count(),
                ReadCount = g.Count(n => n.IsRead),
                UnreadCount = g.Count(n => !n.IsRead),
                NotificationIds = g.Select(n => n.NotificationId).ToList()
            })
            .OrderByDescending(n => n.CreatedAt)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        try
        {
            var deleted = await _notificationService.DeleteMultipleNotificationsAsync(new List<long> { id });
            TempData["SuccessMessage"] = $"Đã xóa {deleted} thông báo thành công!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi khi xóa thông báo: {ex.Message}";
        }
        
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteMultipleAsync(string selectedIds)
    {
        try
        {
            var ids = selectedIds.Split(',')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(long.Parse)
                .ToList();
            
            if (!ids.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một thông báo để xóa";
                return RedirectToPage();
            }

            var deleted = await _notificationService.DeleteMultipleNotificationsAsync(ids);
            TempData["SuccessMessage"] = $"Đã xóa {deleted} thông báo thành công!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Lỗi khi xóa thông báo: {ex.Message}";
        }
        
        return RedirectToPage();
    }

    public class NotificationSummary
    {
        public long NotificationId { get; set; }
        public string? Content { get; set; }
        public string? Type { get; set; }
        public DateTime CreatedAt { get; set; }
        public int RecipientCount { get; set; }
        public int ReadCount { get; set; }
        public int UnreadCount { get; set; }
        public List<long> NotificationIds { get; set; } = new();
    }
}

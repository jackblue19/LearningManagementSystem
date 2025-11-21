using LMS.Data;
using LMS.Hubs;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Communication;
using LMS.Repositories.Interfaces.Info;
using LMS.Services.Interfaces.CommonService;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services.Impl.CommonService;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepo;
    private readonly IUserRepository _userRepo;
    private readonly IClassRegistrationRepository _classRegRepo;
    private readonly CenterDbContext _db;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(
        INotificationRepository notificationRepo,
        IUserRepository userRepo,
        IClassRegistrationRepository classRegRepo,
        CenterDbContext db,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepo = notificationRepo;
        _userRepo = userRepo;
        _classRegRepo = classRegRepo;
        _db = db;
        _hubContext = hubContext;
    }

    public async Task<Notification> SendNotificationAsync(
        Guid userId, 
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Content = $"{title}\n{message}",
            NotiType = type,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        await _notificationRepo.AddAsync(notification, saveNow: true, ct);
        
        // Send real-time notification via SignalR
        await _hubContext.Clients.Group($"user-{userId}")
            .SendAsync("ReceiveNotification", new
            {
                notificationId = notification.NotificationId,
                title,
                message,
                type,
                createdAt = notification.CreatedAt.ToString("o")
            }, ct);
        
        return notification;
    }

    public async Task<List<Notification>> SendBulkNotificationAsync(
        List<Guid> userIds, 
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default)
    {
        // Log original count for debugging
        var originalCount = userIds.Count;
        
        // Remove duplicates
        userIds = userIds.Distinct().ToList();
        
        Console.WriteLine($"[NotificationService] SendBulkNotificationAsync called");
        Console.WriteLine($"[NotificationService] Original userIds count: {originalCount}, After Distinct: {userIds.Count}");
        
        // Validate userIds - remove empty/null guids
        var validUserIds = userIds.Where(id => id != Guid.Empty).ToList();
        if (validUserIds.Count != userIds.Count)
        {
            Console.WriteLine($"[NotificationService] WARNING: Found {userIds.Count - validUserIds.Count} empty/invalid user IDs. Removing them.");
            userIds = validUserIds;
        }
        
        // Log userIds safely
        for (int i = 0; i < userIds.Count; i++)
        {
            Console.WriteLine($"[NotificationService] UserIds[{i}]: {userIds[i]}");
        }
        
        // Log if duplicates were found
        if (originalCount != userIds.Count)
        {
            Console.WriteLine($"[NotificationService] WARNING: Found {originalCount - userIds.Count} duplicate user IDs.");
        }
        
        var notifications = new List<Notification>();
        
        // Create notifications ONE BY ONE to ensure correct mapping
        foreach (var userId in userIds)
        {
            var notification = new Notification
            {
                UserId = userId,
                Content = $"{title}\n{message}",
                NotiType = type,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            notifications.Add(notification);
            Console.WriteLine($"[NotificationService] Created notification for UserId: {userId}");
        }
        
        Console.WriteLine($"[NotificationService] Created {notifications.Count} notification objects");
        
        // Batch insert all notifications in one transaction
        await _notificationRepo.AddRangeAsync(notifications, saveNow: true, ct);
        
        Console.WriteLine($"[NotificationService] AddRangeAsync completed. Verifying data...");
        for (int i = 0; i < notifications.Count; i++)
        {
            Console.WriteLine($"[NotificationService] Notification[{i}] - NotificationId: {notifications[i].NotificationId}, UserId: {notifications[i].UserId}");
        }
        
        // Send SignalR notifications after database insert completes
        foreach (var notification in notifications)
        {
            Console.WriteLine($"[NotificationService] Sending SignalR to UserId: {notification.UserId}, NotificationId: {notification.NotificationId}");
            
            await _hubContext.Clients.Group($"user-{notification.UserId}")
                .SendAsync("ReceiveNotification", new
                {
                    notificationId = notification.NotificationId,
                    title,
                    message,
                    type,
                    createdAt = notification.CreatedAt.ToString("o")
                }, ct);
        }
        
        Console.WriteLine($"[NotificationService] Sent {notifications.Count} notifications successfully.");

        return notifications;
    }

    public async Task<List<Notification>> SendToClassAsync(
        Guid classId, 
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default)
    {
        Console.WriteLine($"[NotificationService] SendToClassAsync - ClassId: {classId}");
        
        // Lấy tất cả học sinh trong lớp (với logging)
        var allRegistrations = await _db.ClassRegistrations
            .Where(cr => cr.ClassId == classId && cr.RegistrationStatus == "approved")
            .ToListAsync(ct);
        
        Console.WriteLine($"[NotificationService] Found {allRegistrations.Count} approved registrations");
        
        var studentIds = allRegistrations
            .Select(cr => cr.StudentId)
            .ToList();
        
        var distinctStudentIds = studentIds.Distinct().ToList();
        
        if (studentIds.Count != distinctStudentIds.Count)
        {
            Console.WriteLine($"[NotificationService] WARNING: Found duplicate StudentIds in ClassRegistrations! Total: {studentIds.Count}, Unique: {distinctStudentIds.Count}");
            // Log duplicate StudentIds
            var duplicates = studentIds.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
            Console.WriteLine($"[NotificationService] Duplicate StudentIds: {string.Join(", ", duplicates)}");
        }

        if (!distinctStudentIds.Any())
        {
            Console.WriteLine($"[NotificationService] No students found for class {classId}");
            return new List<Notification>();
        }

        Console.WriteLine($"[NotificationService] Sending to {distinctStudentIds.Count} unique students");
        return await SendBulkNotificationAsync(distinctStudentIds, title, message, type, ct);
    }

    public async Task<List<Notification>> SendToAllTeachersAsync(
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default)
    {
        Console.WriteLine($"[NotificationService] SendToAllTeachersAsync");
        
        var allTeachers = await _db.Users
            .Where(u => u.RoleDesc == "teacher")
            .ToListAsync(ct);
        
        Console.WriteLine($"[NotificationService] Found {allTeachers.Count} teachers");
        
        var teacherIds = allTeachers.Select(u => u.UserId).Distinct().ToList();

        if (!teacherIds.Any())
        {
            Console.WriteLine($"[NotificationService] No teachers found");
            return new List<Notification>();
        }

        Console.WriteLine($"[NotificationService] Sending to {teacherIds.Count} teachers");
        return await SendBulkNotificationAsync(teacherIds, title, message, type, ct);
    }

    public async Task<List<Notification>> SendToAllStudentsAsync(
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default)
    {
        Console.WriteLine($"[NotificationService] SendToAllStudentsAsync");
        
        var allStudents = await _db.Users
            .Where(u => u.RoleDesc == "student")
            .ToListAsync(ct);
        
        Console.WriteLine($"[NotificationService] Found {allStudents.Count} students");
        
        var studentIds = allStudents.Select(u => u.UserId).Distinct().ToList();

        if (!studentIds.Any())
        {
            Console.WriteLine($"[NotificationService] No students found");
            return new List<Notification>();
        }

        Console.WriteLine($"[NotificationService] Sending to {studentIds.Count} students");
        return await SendBulkNotificationAsync(studentIds, title, message, type, ct);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(
        Guid userId, 
        int pageNumber = 1, 
        int pageSize = 20, 
        CancellationToken ct = default)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(ct);
    }

    public async Task<bool> MarkAsReadAsync(long notificationId, CancellationToken ct = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct);
        if (notification == null) return false;

        notification.IsRead = true;
        await _notificationRepo.UpdateAsync(notification, saveNow: true, ct);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default)
    {
        var notifications = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            await _notificationRepo.UpdateAsync(notification, saveNow: true, ct);
        }

        return true;
    }

    public async Task<bool> DeleteNotificationAsync(long notificationId, CancellationToken ct = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct);
        if (notification == null) return false;

        await _notificationRepo.DeleteAsync(notification, saveNow: true, ct);
        return true;
    }

    public async Task<List<Notification>> GetAllNotificationsAsync(CancellationToken ct = default)
    {
        return await _db.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Notification?> GetNotificationByIdAsync(long notificationId, CancellationToken ct = default)
    {
        return await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct);
    }

    public async Task<bool> UpdateNotificationAsync(
        long notificationId, 
        string title, 
        string message, 
        string type, 
        CancellationToken ct = default)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId, ct);
        
        if (notification == null) return false;

        notification.Content = $"{title}\n{message}";
        notification.NotiType = type;

        await _notificationRepo.UpdateAsync(notification, saveNow: true, ct);

        // Send real-time update via SignalR
        if (notification.UserId.HasValue)
        {
            await _hubContext.Clients.Group($"user-{notification.UserId.Value}")
                .SendAsync("NotificationUpdated", new
                {
                    notificationId = notification.NotificationId,
                    title,
                    message,
                    type,
                    updatedAt = DateTime.Now.ToString("o")
                }, ct);
        }

        return true;
    }

    public async Task<int> DeleteMultipleNotificationsAsync(List<long> notificationIds, CancellationToken ct = default)
    {
        var notifications = await _db.Notifications
            .Where(n => notificationIds.Contains(n.NotificationId))
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            await _notificationRepo.DeleteAsync(notification, saveNow: true, ct);
            
            // Notify user via SignalR
            if (notification.UserId.HasValue)
            {
                await _hubContext.Clients.Group($"user-{notification.UserId.Value}")
                    .SendAsync("NotificationDeleted", notification.NotificationId, ct);
            }
        }

        return notifications.Count;
    }
}

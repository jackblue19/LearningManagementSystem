using LMS.Models.Entities;

namespace LMS.Services.Interfaces.CommonService;

public interface INotificationService
{
    // Gửi thông báo cho một người
    Task<Notification> SendNotificationAsync(
        Guid userId, 
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default);

    // Gửi thông báo cho nhiều người
    Task<List<Notification>> SendBulkNotificationAsync(
        List<Guid> userIds, 
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default);

    // Gửi thông báo cho tất cả học sinh trong một lớp
    Task<List<Notification>> SendToClassAsync(
        Guid classId, 
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default);

    // Gửi thông báo cho tất cả giáo viên
    Task<List<Notification>> SendToAllTeachersAsync(
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default);

    // Gửi thông báo cho tất cả học sinh
    Task<List<Notification>> SendToAllStudentsAsync(
        string title, 
        string message, 
        string type = "info", 
        CancellationToken ct = default);

    // Lấy thông báo của user
    Task<List<Notification>> GetUserNotificationsAsync(
        Guid userId, 
        int pageNumber = 1, 
        int pageSize = 20, 
        CancellationToken ct = default);

    // Đếm thông báo chưa đọc
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    // Đánh dấu đã đọc
    Task<bool> MarkAsReadAsync(long notificationId, CancellationToken ct = default);

    // Đánh dấu tất cả đã đọc
    Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken ct = default);

    // Xóa thông báo
    Task<bool> DeleteNotificationAsync(long notificationId, CancellationToken ct = default);

    // Lấy tất cả thông báo (cho Manager)
    Task<List<Notification>> GetAllNotificationsAsync(CancellationToken ct = default);

    // Lấy thông báo theo ID
    Task<Notification?> GetNotificationByIdAsync(long notificationId, CancellationToken ct = default);

    // Cập nhật thông báo
    Task<bool> UpdateNotificationAsync(long notificationId, string title, string message, string type, CancellationToken ct = default);

    // Xóa nhiều thông báo
    Task<int> DeleteMultipleNotificationsAsync(List<long> notificationIds, CancellationToken ct = default);
}

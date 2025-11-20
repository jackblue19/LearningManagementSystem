using LMS.Data;
using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using Microsoft.EntityFrameworkCore;

namespace LMS.Services.Impl.AdminService;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly CenterDbContext _db;

    public AdminDashboardService(CenterDbContext db)
    {
        _db = db;
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        var totalUsers = await _db.Users.CountAsync(ct);
        var activeUsers = await _db.Users.CountAsync(u => u.IsActive, ct);
        var pendingUsers = await _db.Users.CountAsync(u => !u.IsActive, ct);

        var totalClasses = await _db.Classes.CountAsync(ct);
        var activeClasses = await _db.Classes.CountAsync(c => c.ClassStatus != null && c.ClassStatus != "finished", ct);

        var totalFeedbacks = await _db.Feedbacks.CountAsync(ct);
        var pendingFeedbacks = await _db.Feedbacks.CountAsync(f => f.FbStatus == "pending", ct);
        var hiddenFeedbacks = await _db.Feedbacks.CountAsync(f => f.IsVisible == false, ct);

        var totalAuditLogs = await _db.AuditLogs.CountAsync(ct);

        var recentFeedbacks = await _db.Feedbacks
            .AsNoTracking()
            .Include(f => f.User)
            .Include(f => f.Class)
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .Select(f => new RecentFeedbackItem
            {
                FeedbackId = f.FeedbackId,
                Username = f.User.Username,
                FullName = f.User.FullName,
                ClassName = f.Class.ClassName,
                Rating = f.Rating,
                FbStatus = f.FbStatus,
                IsVisible = f.IsVisible ?? true,
                CreatedAt = f.CreatedAt,
                Content = f.Content
            })
            .ToListAsync(ct);

        var recentAuditLogs = await _db.AuditLogs
            .AsNoTracking()
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new RecentAuditLogItem
            {
                LogId = a.LogId,
                Username = a.User.Username,
                FullName = a.User.FullName,
                ActionType = a.ActionType,
                EntityName = a.EntityName,
                RecordId = a.RecordId,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(ct);

        return new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            PendingUsers = pendingUsers,
            TotalClasses = totalClasses,
            ActiveClasses = activeClasses,
            TotalFeedbacks = totalFeedbacks,
            PendingFeedbacks = pendingFeedbacks,
            HiddenFeedbacks = hiddenFeedbacks,
            TotalAuditLogs = totalAuditLogs,
            RecentFeedbacks = recentFeedbacks,
            RecentAuditLogs = recentAuditLogs
        };
    }
}


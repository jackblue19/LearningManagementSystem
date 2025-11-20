using System;

namespace LMS.Models.ViewModels.Admin;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int PendingUsers { get; set; }

    public int TotalClasses { get; set; }
    public int ActiveClasses { get; set; }

    public int TotalFeedbacks { get; set; }
    public int PendingFeedbacks { get; set; }
    public int HiddenFeedbacks { get; set; }

    public int TotalAuditLogs { get; set; }

    public IReadOnlyList<RecentFeedbackItem> RecentFeedbacks { get; set; } = Array.Empty<RecentFeedbackItem>();
    public IReadOnlyList<RecentAuditLogItem> RecentAuditLogs { get; set; } = Array.Empty<RecentAuditLogItem>();
}

public class RecentFeedbackItem
{
    public long FeedbackId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public string? FbStatus { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Content { get; set; }
}

public class RecentAuditLogItem
{
    public long LogId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? ActionType { get; set; }
    public string? EntityName { get; set; }
    public string? RecordId { get; set; }
    public DateTime CreatedAt { get; set; }
}


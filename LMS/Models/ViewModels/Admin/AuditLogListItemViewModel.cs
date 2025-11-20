namespace LMS.Models.ViewModels.Admin;

public class AuditLogListItemViewModel
{
    public long LogId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? ActionType { get; set; }
    public string? EntityName { get; set; }
    public string? RecordId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? OldData { get; set; }
    public string? NewData { get; set; }
}


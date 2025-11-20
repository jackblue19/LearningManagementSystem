using System;

namespace LMS.Models.ViewModels.Admin;

public class FeedbackListItemViewModel
{
    public long FeedbackId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string? Content { get; set; }
    public int? Rating { get; set; }
    public string? FbStatus { get; set; }
    public bool IsVisible { get; set; }
    public DateTime CreatedAt { get; set; }
}


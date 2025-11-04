using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Areas.Admin.Pages.Users;

public class IndexModel : PageModel
{
    public IReadOnlyList<UserDirectoryViewModel> Users { get; private set; } = Array.Empty<UserDirectoryViewModel>();
    public IReadOnlyList<NotificationViewModel> Notifications { get; private set; } = Array.Empty<NotificationViewModel>();
    public IReadOnlyList<AuditLogEntryViewModel> AuditLog { get; private set; } = Array.Empty<AuditLogEntryViewModel>();

    public int ActiveUsers => Users.Count(u => u.IsActive);
    public int NewUsers => Users.Count(u => u.LastActiveAt >= DateTime.UtcNow.AddDays(-30));
    public int UniqueRoles => Users.Select(u => u.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count();

    public void OnGet()
    {
        Users = new List<UserDirectoryViewModel>
        {
            new("Alicia Graham", "Admin", "alicia.graham@example.com", DateTime.UtcNow.AddHours(-4), true),
            new("Rahul Patel", "Teacher", "rahul.patel@example.com", DateTime.UtcNow.AddHours(-20), true),
            new("Morgan Lee", "Teacher", "morgan.lee@example.com", DateTime.UtcNow.AddDays(-6), true),
            new("Grace Park", "Student", "grace.park@example.com", DateTime.UtcNow.AddDays(-3), true),
            new("Andre Lopez", "Student", "andre.lopez@example.com", DateTime.UtcNow.AddDays(-18), false)
        };

        Notifications = new List<NotificationViewModel>
        {
            new("Policy acknowledgement", "All users", "Apr 02, 07:45", "Active"),
            new("Role change approvals", "Admin team", "Apr 01, 10:30", "Sent"),
            new("Dormant account review", "Support", "Mar 29, 09:00", "Queued")
        };

        AuditLog = new List<AuditLogEntryViewModel>
        {
            new(DateTime.UtcNow.AddHours(-2), "Alicia Graham", "Granted role", "Grace Park → Student", "Web"),
            new(DateTime.UtcNow.AddHours(-5), "System", "Deactivated account", "Andre Lopez", "Automation"),
            new(DateTime.UtcNow.AddHours(-10), "Rahul Patel", "Updated profile", "Morgan Lee", "Portal")
        };
    }
}

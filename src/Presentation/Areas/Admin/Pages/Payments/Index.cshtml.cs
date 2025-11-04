using Microsoft.AspNetCore.Mvc.RazorPages;
using Presentation.ViewModels.Admin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Presentation.Areas.Admin.Pages.Payments;

public class IndexModel : PageModel
{
    public IReadOnlyList<PaymentRecordViewModel> Payments { get; private set; } = Array.Empty<PaymentRecordViewModel>();
    public IReadOnlyList<NotificationViewModel> Notifications { get; private set; } = Array.Empty<NotificationViewModel>();
    public IReadOnlyList<AuditLogEntryViewModel> AuditLog { get; private set; } = Array.Empty<AuditLogEntryViewModel>();

    public decimal TotalCollected => Payments.Where(p => string.Equals(p.Status, "Settled", StringComparison.OrdinalIgnoreCase)).Sum(p => p.Amount);
    public decimal TotalOutstanding => Payments.Where(p => !string.Equals(p.Status, "Settled", StringComparison.OrdinalIgnoreCase)).Sum(p => p.Amount);

    public decimal CollectionRate
    {
        get
        {
            var issued = Payments.Sum(p => p.Amount);
            return issued == 0 ? 0 : Math.Round(TotalCollected * 100m / issued, 1);
        }
    }

    public void OnGet()
    {
        Payments = new List<PaymentRecordViewModel>
        {
            new("PMT-98231", "Sabrina James", "Tuition", 1850m, DateTime.UtcNow.AddDays(-2), "Settled"),
            new("PMT-98232", "Andre Lopez", "Program fee", 620m, DateTime.UtcNow.AddDays(-1), "Pending"),
            new("PMT-98233", "Nova Learning", "Corporate", 9200m, DateTime.UtcNow.AddDays(-5), "Settled"),
            new("PMT-98234", "Grace Park", "Scholarship", 450m, DateTime.UtcNow.AddDays(-3), "Under review")
        };

        Notifications = new List<NotificationViewModel>
        {
            new("Payment reminder batch", "Students", "Apr 02, 07:00", "Scheduled"),
            new("Corporate invoice paid", "Finance", "Mar 31, 11:20", "Sent"),
            new("Scholarship disbursement", "Operations", "Mar 29, 15:45", "Sent")
        };

        AuditLog = new List<AuditLogEntryViewModel>
        {
            new(DateTime.UtcNow.AddHours(-2), "Finance automation", "Marked payment settled", "PMT-98231", "API"),
            new(DateTime.UtcNow.AddHours(-6), "Andre Lopez", "Updated billing details", "PMT-98232", "Portal"),
            new(DateTime.UtcNow.AddHours(-12), "Grace Park", "Submitted documentation", "PMT-98234", "Portal")
        };
    }
}

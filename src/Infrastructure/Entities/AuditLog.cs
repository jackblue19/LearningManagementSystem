using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Entities;

public partial class AuditLog
{
    [Key]
    public long LogId { get; set; }

    public Guid UserId { get; set; }

    [StringLength(40)]
    public string? ActionType { get; set; }

    [StringLength(100)]
    public string? EntityName { get; set; }

    [StringLength(100)]
    public string? RecordId { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public string? NewData { get; set; }

    public string? OldData { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("AuditLogs")]
    public virtual User User { get; set; } = null!;
}

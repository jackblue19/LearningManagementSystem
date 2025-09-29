using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class Notification
{
    [Key]
    public long NotificationId { get; set; }

    public Guid? UserId { get; set; }

    public Guid? ClassId { get; set; }

    public Guid? CenterId { get; set; }

    [StringLength(400)]
    public string? Content { get; set; }

    [StringLength(40)]
    public string? NotiType { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("Notifications")]
    public virtual Center? Center { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("Notifications")]
    public virtual Class? Class { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Notifications")]
    public virtual User? User { get; set; }
}

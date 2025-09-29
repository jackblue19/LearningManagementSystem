using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class Center
{
    [Key]
    public Guid CenterId { get; set; }

    [StringLength(200)]
    public string CenterName { get; set; } = null!;

    [StringLength(300)]
    public string? CenterAddress { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    [StringLength(120)]
    public string? CenterEmail { get; set; }

    [StringLength(300)]
    public string? Logo { get; set; }

    public Guid ManagerId { get; set; }

    [InverseProperty("Center")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [ForeignKey("ManagerId")]
    [InverseProperty("Centers")]
    public virtual User Manager { get; set; } = null!;

    [InverseProperty("Center")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Center")]
    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}

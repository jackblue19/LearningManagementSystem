using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class Class
{
    [Key]
    public Guid ClassId { get; set; }

    [StringLength(150)]
    public string ClassName { get; set; } = null!;

    public long SubjectId { get; set; }

    public Guid TeacherId { get; set; }

    [StringLength(300)]
    public string? ClassAddress { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? UnitPrice { get; set; }

    public int? TotalSessions { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(500)]
    public string? ScheduleDesc { get; set; }

    [StringLength(300)]
    public string? CoverImageUrl { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public Guid CenterId { get; set; }

    [StringLength(40)]
    public string? ClassStatus { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("Classes")]
    public virtual Center Center { get; set; } = null!;

    [InverseProperty("Class")]
    public virtual ICollection<ClassMaterial> ClassMaterials { get; set; } = new List<ClassMaterial>();

    [InverseProperty("Class")]
    public virtual ICollection<ClassRegistration> ClassRegistrations { get; set; } = new List<ClassRegistration>();

    [InverseProperty("Class")]
    public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();

    [InverseProperty("Class")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("Class")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("Class")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Class")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("SubjectId")]
    [InverseProperty("Classes")]
    public virtual Subject Subject { get; set; } = null!;

    [ForeignKey("TeacherId")]
    [InverseProperty("Classes")]
    public virtual User Teacher { get; set; } = null!;
}

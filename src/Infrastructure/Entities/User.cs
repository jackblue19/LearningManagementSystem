using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Entities;

public partial class User
{
    [Key]
    public Guid UserId { get; set; }

    [StringLength(100)]
    public string Username { get; set; } = null!;

    [StringLength(120)]
    public string Email { get; set; } = null!;

    [StringLength(256)]
    public string PasswordHash { get; set; } = null!;

    [StringLength(120)]
    public string? FullName { get; set; }

    [StringLength(300)]
    public string? Avatar { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(40)]
    public string? RoleDesc { get; set; }

    public bool IsActive { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? UpdatedAt { get; set; }

    [StringLength(300)]
    public string? CoverImageUrl { get; set; }

    [InverseProperty("Student")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [InverseProperty("User")]
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    [InverseProperty("Manager")]
    public virtual ICollection<Center> Centers { get; set; } = new List<Center>();

    [InverseProperty("UploadedByUser")]
    public virtual ICollection<ClassMaterial> ClassMaterials { get; set; } = new List<ClassMaterial>();

    [InverseProperty("Student")]
    public virtual ICollection<ClassRegistration> ClassRegistrations { get; set; } = new List<ClassRegistration>();

    [InverseProperty("Teacher")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    [InverseProperty("Student")]
    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

    [InverseProperty("Teacher")]
    public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

    [InverseProperty("User")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [InverseProperty("Student")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

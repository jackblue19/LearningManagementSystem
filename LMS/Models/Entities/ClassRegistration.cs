using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

public partial class ClassRegistration
{
    [Key]
    public long RegistrationId { get; set; }

    public Guid ClassId { get; set; }

    public Guid StudentId { get; set; }

    [Precision(0)]
    public DateTime RegisteredAt { get; set; }

    [StringLength(40)]
    public string? RegistrationStatus { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("ClassRegistrations")]
    public virtual Class Class { get; set; } = null!;

    [InverseProperty("Registration")]
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    [InverseProperty("Registration")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("StudentId")]
    [InverseProperty("ClassRegistrations")]
    public virtual User Student { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class Feedback
{
    [Key]
    public long FeedbackId { get; set; }

    public Guid UserId { get; set; }

    public Guid ClassId { get; set; }

    public string? Content { get; set; }

    [StringLength(40)]
    public string? FbStatus { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public int? Rating { get; set; }

    public bool? IsVisible { get; set; }

    public long? RegistrationId { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("Feedbacks")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("RegistrationId")]
    [InverseProperty("Feedbacks")]
    public virtual ClassRegistration? Registration { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Feedbacks")]
    public virtual User User { get; set; } = null!;
}

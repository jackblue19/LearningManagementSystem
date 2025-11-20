using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

public partial class Payment
{
    [Key]
    public Guid PaymentId { get; set; }

    public Guid StudentId { get; set; }

    public Guid? ClassId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [StringLength(40)]
    public string? PaymentStatus { get; set; }

    [StringLength(40)]
    public string? PaymentMethod { get; set; }
    public string? VnpTxnRef { get; set; }
    public string? BankCode { get; set; }
    public string? TnxNo { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Precision(0)]
    public DateTime? PaidAt { get; set; }

    public long? RegistrationId { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("Payments")]
    public virtual Class? Class { get; set; }

    [ForeignKey("RegistrationId")]
    [InverseProperty("Payments")]
    public virtual ClassRegistration? Registration { get; set; }

    [ForeignKey("StudentId")]
    [InverseProperty("Payments")]
    public virtual User Student { get; set; } = null!;
}

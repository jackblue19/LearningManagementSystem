using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

public partial class ExamResult
{
    [Key]
    public long ResultId { get; set; }

    public Guid ExamId { get; set; }

    public Guid StudentId { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? Score { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }

    [ForeignKey("ExamId")]
    [InverseProperty("ExamResults")]
    public virtual Exam Exam { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("ExamResults")]
    public virtual User Student { get; set; } = null!;
}

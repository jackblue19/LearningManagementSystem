using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class Exam
{
    [Key]
    public Guid ExamId { get; set; }

    public Guid ClassId { get; set; }

    [StringLength(150)]
    public string Title { get; set; } = null!;

    [StringLength(40)]
    public string? ExamType { get; set; }

    public DateOnly? ExamDate { get; set; }

    public Guid TeacherId { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal? MaxScore { get; set; }

    public int? DurationMin { get; set; }

    [StringLength(500)]
    public string? ExamDesc { get; set; }

    [StringLength(40)]
    public string? ExamStatus { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("Exams")]
    public virtual Class Class { get; set; } = null!;

    [InverseProperty("Exam")]
    public virtual ICollection<ClassMaterial> ClassMaterials { get; set; } = new List<ClassMaterial>();

    [InverseProperty("Exam")]
    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

    [ForeignKey("TeacherId")]
    [InverseProperty("Exams")]
    public virtual User Teacher { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class ClassMaterial
{
    [Key]
    public Guid MaterialId { get; set; }

    public Guid ClassId { get; set; }

    [StringLength(150)]
    public string? Title { get; set; }

    [StringLength(500)]
    public string? FileUrl { get; set; }

    public Guid UploadedByUserId { get; set; }

    [Precision(0)]
    public DateTime UploadedAt { get; set; }

    [StringLength(40)]
    public string? MaterialType { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }

    public Guid? ExamId { get; set; }

    public long? FileSize { get; set; }

    [StringLength(300)]
    public string? CloudObjectKey { get; set; }

    [ForeignKey("ClassId")]
    [InverseProperty("ClassMaterials")]
    public virtual Class Class { get; set; } = null!;

    [ForeignKey("ExamId")]
    [InverseProperty("ClassMaterials")]
    public virtual Exam? Exam { get; set; }

    [ForeignKey("UploadedByUserId")]
    [InverseProperty("ClassMaterials")]
    public virtual User UploadedByUser { get; set; } = null!;
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

public partial class Subject
{
    [Key]
    public long SubjectId { get; set; }

    [StringLength(120)]
    public string SubjectName { get; set; } = null!;

    [StringLength(40)]
    public string? GradeLevel { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    public Guid CenterId { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("Subjects")]
    public virtual Center Center { get; set; } = null!;

    [InverseProperty("Subject")]
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}

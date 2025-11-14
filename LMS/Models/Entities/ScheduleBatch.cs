using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

public partial class ScheduleBatch
{
    [Key]
    public Guid BatchId { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    [InverseProperty("Batch")]
    public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}

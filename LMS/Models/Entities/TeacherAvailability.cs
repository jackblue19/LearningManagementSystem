using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

[Index("TeacherId", "DayOfWeek", "StartTime", "EndTime", Name = "IX_TeacherAvail")]
public partial class TeacherAvailability
{
    [Key]
    public long AvailabilityId { get; set; }

    public Guid TeacherId { get; set; }

    public byte DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [StringLength(150)]
    public string? Note { get; set; }

    [ForeignKey("TeacherId")]
    [InverseProperty("TeacherAvailabilities")]
    public virtual User Teacher { get; set; } = null!;

}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

public partial class Attendance
{
    [Key]
    public long AttendanceId { get; set; }

    public long ScheduleId { get; set; }

    public Guid StudentId { get; set; }

    [StringLength(40)]
    public string? StudentStatus { get; set; }

    [StringLength(300)]
    public string? Note { get; set; }

    [ForeignKey("ScheduleId")]
    [InverseProperty("Attendances")]
    public virtual ClassSchedule Schedule { get; set; } = null!;

    [ForeignKey("StudentId")]
    [InverseProperty("Attendances")]
    public virtual User Student { get; set; } = null!;
}

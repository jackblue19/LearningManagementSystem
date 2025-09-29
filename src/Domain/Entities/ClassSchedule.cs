using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Domain.Entities;

public partial class ClassSchedule
{
    [Key]
    public long ScheduleId { get; set; }

    public Guid ClassId { get; set; }

    public DateOnly SessionDate { get; set; }

    [StringLength(100)]
    public string? RoomName { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public int? SlotOrder { get; set; }

    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [StringLength(150)]
    public string? ScheduleLabel { get; set; }

    [StringLength(150)]
    public string? ScheduleNote { get; set; }

    [InverseProperty("Schedule")]
    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    [ForeignKey("ClassId")]
    [InverseProperty("ClassSchedules")]
    public virtual Class Class { get; set; } = null!;
}

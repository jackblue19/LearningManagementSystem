using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

[Index("SlotOrder", Name = "UQ__TimeSlot__F57364B47815BB11", IsUnique = true)]
public partial class TimeSlot
{
    [Key]
    public byte SlotId { get; set; }

    [StringLength(40)]
    public string SlotLabel { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public byte? SlotOrder { get; set; }

    [InverseProperty("Slot")]
    public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();
}

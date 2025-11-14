using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

[Index("RoomId", "DayOfWeek", "StartTime", "EndTime", Name = "IX_RoomAvail")]
public partial class RoomAvailability
{
    [Key]
    public long AvailabilityId { get; set; }

    public Guid RoomId { get; set; }

    public byte DayOfWeek { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    [StringLength(150)]
    public string? Note { get; set; }

    [ForeignKey("RoomId")]
    [InverseProperty("RoomAvailabilities")]
    public virtual Room Room { get; set; } = null!;
}

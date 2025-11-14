using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LMS.Models.Entities;

[Index("CenterId", "RoomName", Name = "UQ_Room_Center", IsUnique = true)]
public partial class Room
{
    [Key]
    public Guid RoomId { get; set; }

    public Guid CenterId { get; set; }

    [StringLength(120)]
    public string RoomName { get; set; } = null!;

    public int? Capacity { get; set; }

    public bool IsActive { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("Rooms")]
    public virtual Center Center { get; set; } = null!;

    [InverseProperty("Room")]
    public virtual ICollection<ClassSchedule> ClassSchedules { get; set; } = new List<ClassSchedule>();

    [InverseProperty("Room")]
    public virtual ICollection<RoomAvailability> RoomAvailabilities { get; set; } = new List<RoomAvailability>();
}

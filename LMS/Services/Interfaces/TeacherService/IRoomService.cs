using System;
using LMS.Models.Entities;
using LMS.Services.Interfaces;

namespace LMS.Services.Interfaces.TeacherService;

public interface IRoomService : ICrudService<Room, Guid>
{
}

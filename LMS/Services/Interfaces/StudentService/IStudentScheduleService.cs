using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LMS.Models.Entities;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentScheduleService
{
    Task<IReadOnlyList<ClassSchedule>> GetClassScheduleAsync(
        Guid classId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);
}

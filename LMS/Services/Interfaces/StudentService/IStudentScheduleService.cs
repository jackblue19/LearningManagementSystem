using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LMS.Models.Entities;

using LMS.Models.ViewModels.StudentService;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentScheduleService
{
    Task<IReadOnlyList<ClassSchedule>> GetClassScheduleAsync(
        Guid classId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<StudentScheduleItemVm>> GetScheduleAsyncZ(
           Guid studentId, DateOnly from, DateOnly to, CancellationToken ct = default);
           
    Task<IReadOnlyList<ClassSchedule>> GetScheduleAsync(
        Guid studentId,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken ct = default);
}

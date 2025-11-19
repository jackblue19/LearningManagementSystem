using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Services.Impl.StudentService;

public class StudentScheduleService : IStudentScheduleService
{
    private readonly IClassScheduleRepository _classScheduleRepository;

    public StudentScheduleService(IClassScheduleRepository classScheduleRepository)
    {
        _classScheduleRepository = classScheduleRepository;
    }

    public Task<IReadOnlyList<ClassSchedule>> GetClassScheduleAsync(
        Guid classId,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken ct = default)
        => _classScheduleRepository.GetByClassAsync(classId, from, to, ct);
}

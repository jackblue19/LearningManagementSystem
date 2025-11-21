using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Impl;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class TeacherAvailabilityService : CrudService<TeacherAvailability, long>, ITeacherAvailabilityService
{
    private readonly ITeacherAvailabilityRepository _teacherAvailabilityRepository;

    public TeacherAvailabilityService(ITeacherAvailabilityRepository teacherAvailabilityRepository)
        : base(teacherAvailabilityRepository)
    {
        _teacherAvailabilityRepository = teacherAvailabilityRepository;
    }
}

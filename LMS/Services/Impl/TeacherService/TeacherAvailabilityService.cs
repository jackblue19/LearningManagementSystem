using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class TeacherAvailabilityService : ITeacherAvailabilityService
{
    private readonly ITeacherAvailabilityRepository _teacherAvailRepo;

    public TeacherAvailabilityService(ITeacherAvailabilityRepository teacherAvailRepo)
    {
        _teacherAvailRepo = teacherAvailRepo;
    }
}

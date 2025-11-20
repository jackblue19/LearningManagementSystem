using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class TimeSlotService : ITimeSlotService
{
    private readonly ITimeSlotRepository _timeSlotRepo;

    public TimeSlotService(ITimeSlotRepository timeSlotRepo)
    {
        _timeSlotRepo = timeSlotRepo;
    }
}

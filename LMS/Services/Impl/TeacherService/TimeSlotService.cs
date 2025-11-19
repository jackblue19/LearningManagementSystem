using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Impl;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Services.Impl.TeacherService;

public class TimeSlotService : CrudService<TimeSlot, byte>, ITimeSlotService
{
    private readonly ITimeSlotRepository _timeSlotRepository;

    public TimeSlotService(ITimeSlotRepository timeSlotRepository)
        : base(timeSlotRepository)
    {
        _timeSlotRepository = timeSlotRepository;
    }
}

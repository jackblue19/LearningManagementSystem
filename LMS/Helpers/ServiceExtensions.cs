using LMS.Services.Impl.CommonService;
using LMS.Services.Impl.ManagerService;
using LMS.Services.Impl.TeacherService;
using LMS.Services.Interfaces.CommonService;
using LMS.Services.Interfaces.ManagerService;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Common Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<EmailHelper>();

        // Teacher Services
        services.AddScoped<IMaterialService, MaterialService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IClassScheduleService, ClassScheduleService>();
        services.AddScoped<IClassManagementService, ClassManagementService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IExamResultService, ExamResultService>();
        services.AddScoped<ITimeSlotService, TimeSlotService>();
        services.AddScoped<ITeacherAvailabilityService, TeacherAvailabilityService>();

        // Manager Services
        services.AddScoped<ITeacherManagementService, TeacherManagementService>();

        return services;
    }
}

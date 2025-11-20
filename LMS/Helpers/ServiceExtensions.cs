using LMS.Services.Impl.TeacherService;
using LMS.Services.Interfaces.TeacherService;

namespace LMS.Helpers;

public static class ServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Common Services
        services.AddScoped<Services.Interfaces.CommonService.IAuthService, Services.Impl.CommonService.AuthService>();
        services.AddScoped<Services.Interfaces.CommonService.INotificationService, Services.Impl.CommonService.NotificationService>();
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
        services.AddScoped<Services.Interfaces.ManagerService.ITeacherManagementService, Services.Impl.ManagerService.TeacherManagementService>();
        services.AddScoped<Services.Interfaces.ManagerService.IClassService, Services.Impl.ManagerService.ClassService>();

        return services;
    }
}

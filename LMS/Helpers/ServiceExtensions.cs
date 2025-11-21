using LMS.Services.Impl.AdminService;
using LMS.Services.Impl.CommonService;
using LMS.Services.Impl.TeacherService;
using LMS.Services.Interfaces.AdminService;
using LMS.Services.Interfaces.CommonService;
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
        // User Services
        services.AddScoped<IUserService, UserService>();

        // Admin Services
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();

        // Feedback Services
        services.AddScoped<IFeedbackService, FeedbackService>();

        // AuditLog Services
        services.AddScoped<IAuditLogService, AuditLogService>();

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

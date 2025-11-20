using LMS.Services.Impl.StudentService;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Helpers;

public static class StudentServices
{
    public static IServiceCollection AddStudentServices(this IServiceCollection services)
    {
        services.AddScoped<IClassRegistrationService, ClassRegistrationService>();
        services.AddScoped<IStudentCourseService, StudentCourseService>();
        services.AddScoped<IStudentScheduleService, StudentScheduleService>();
        services.AddScoped<IStudentExamService, StudentExamService>();
        services.AddScoped<IStudentExamResultService, StudentExamResultService>();
        services.AddScoped<IPaymentService, PaymentService>();

        return services;
    }
}

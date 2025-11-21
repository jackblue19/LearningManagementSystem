using LMS.Repositories.Impl.Academic;
using LMS.Repositories.Impl.Finance;
using LMS.Repositories.Impl.Scheduling;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Finance;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Impl.StudentService;
using LMS.Services.Impl.TeacherService;
using LMS.Services.Interfaces.StudentService;
using LMS.Services.Interfaces.TeacherService;

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
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IExamResultService, ExamResultService>();

        // repo
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IClassRegistrationRepository, ClassRegistrationRepository>();
        services.AddScoped<IClassScheduleRepository, ClassScheduleRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IClassRegistrationService, ClassRegistrationService>();
        services.AddScoped<IStudentScheduleService, StudentScheduleService>();
        services.AddScoped<IStudentCourseService, StudentCourseService>();
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}

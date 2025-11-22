using LMS.Repositories.Impl.Academic;
using LMS.Repositories.Impl.Finance;
using LMS.Repositories.Impl.Scheduling;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Finance;
using LMS.Repositories.Interfaces.Scheduling;
using LMS.Services.Centers;
using LMS.Services.Impl.StudentService;
using LMS.Services.Impl.TeacherService;
using LMS.Services.Interfaces.StudentService;
using LMS.Services.Interfaces.TeacherService;
using LMS.Services.Student;

namespace LMS.Helpers;

public static class StudentServices
{
    public static IServiceCollection AddStudentServices(this IServiceCollection services)
    {
        services.AddScoped<IClassRegistrationService, ClassRegistrationService>();
        services.AddScoped<IStudentCourseService, StudentCourseService>();
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
        services.AddScoped<IStudentCourseService, StudentCourseService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ICenterBrowseService, CenterBrowseService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IStudentScheduleService, StudentScheduleService>();
        services.AddScoped<IStudentPaymentService, StudentPaymentService>();
        services.AddScoped<IStudentBillingService, StudentBillingService>();    

        return services;
    }
}

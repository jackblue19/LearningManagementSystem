using LMS.Repositories;
using LMS.Repositories.Impl.Academic;
using LMS.Repositories.Impl.Assessment;
using LMS.Repositories.Impl.Communication;
using LMS.Repositories.Impl.Finance;
using LMS.Repositories.Impl.Info;
using LMS.Repositories.Impl.Scheduling;
using LMS.Repositories.Interfaces.Academic;
using LMS.Repositories.Interfaces.Assessment;
using LMS.Repositories.Interfaces.Communication;
using LMS.Repositories.Interfaces.Finance;
using LMS.Repositories.Interfaces.Info;
using LMS.Repositories.Interfaces.Scheduling;

namespace LMS.Helpers;

public static class RepositoryExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        // User Repository

        services.AddScoped<IUserRepository, UserRepository>();

        // Feedback Repository
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();

        // Generic Repository
        services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

        // Audit Log Repository
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();


        // Assessment Repositories
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IExamResultRepository, ExamResultRepository>();

        // Academic Repositories
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IClassRegistrationRepository, ClassRegistrationRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();

        // Scheduling Repositories
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<IClassScheduleRepository, ClassScheduleRepository>();
        services.AddScoped<ITimeSlotRepository, TimeSlotRepository>();
        services.AddScoped<IRoomAvailabilityRepository, RoomAvailabilityRepository>();
        services.AddScoped<ITeacherAvailabilityRepository, TeacherAvailabilityRepository>();
        services.AddScoped<IScheduleBatchRepository, ScheduleBatchRepository>();

        // Info Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICenterRepository, CenterRepository>();

        // Communication Repositories
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Finance Repositories
        services.AddScoped<IPaymentRepository, PaymentRepository>();

        return services;
    }
}

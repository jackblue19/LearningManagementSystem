using System.Linq;
using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Services.Impl.StudentService;

public class StudentCourseService : IStudentCourseService
{
    private readonly IClassRegistrationRepository _registrationRepository;

    private const string StatusCancelled = "Cancelled";

    public StudentCourseService(IClassRegistrationRepository registrationRepository)
    {
        _registrationRepository = registrationRepository;
    }

    public async Task<IReadOnlyList<Class>> GetRegisteredClassesAsync(
        Guid studentId,
        bool includeCancelled = false,
        CancellationToken ct = default)
    {
        Expression<Func<ClassRegistration, bool>> predicate = registration =>
            registration.StudentId == studentId &&
            (includeCancelled ||
             !string.Equals(registration.RegistrationStatus, StatusCancelled, StringComparison.OrdinalIgnoreCase));

        var includes = new Expression<Func<ClassRegistration, object>>[]
        {
            registration => registration.Class
        };

        var registrations = await _registrationRepository.ListAsync(
            predicate: predicate,
            includes: includes,
            ct: ct);

        return registrations
            .Where(reg => reg.Class is not null)
            .Select(reg => reg.Class)
            .ToList();
    }
}

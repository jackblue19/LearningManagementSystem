using LMS.Models.Entities;
using LMS.Repositories.Interfaces.Academic;
using LMS.Services.Interfaces.StudentService;

namespace LMS.Services.Impl.StudentService;

public class ClassRegistrationService : IClassRegistrationService
{
    private readonly IClassRepository _classRepository;
    private readonly IClassRegistrationRepository _registrationRepository;

    private const string StatusActive = "Active";
    private const string StatusCancelled = "Cancelled";

    public ClassRegistrationService(
        IClassRepository classRepository,
        IClassRegistrationRepository registrationRepository)
    {
        _classRepository = classRepository;
        _registrationRepository = registrationRepository;
    }

    public async Task<bool> RegisterAsync(Guid studentId, Guid classId, CancellationToken ct = default)
    {
        var targetClass = await _classRepository.GetByIdAsync(classId, ct: ct);
        if (targetClass is null) return false;

        var registration = await _registrationRepository.FirstOrDefaultAsync(
            r => r.ClassId == classId && r.StudentId == studentId,
            asNoTracking: false,
            ct: ct);

        if (registration is null)
        {
            registration = new ClassRegistration
            {
                StudentId = studentId,
                ClassId = classId,
                RegisteredAt = DateTime.UtcNow,
                RegistrationStatus = StatusActive
            };

            await _registrationRepository.AddAsync(registration, ct: ct);
            return true;
        }

        if (string.Equals(registration.RegistrationStatus, StatusCancelled, StringComparison.OrdinalIgnoreCase))
        {
            registration.RegistrationStatus = StatusActive;
            registration.RegisteredAt = DateTime.UtcNow;
            await _registrationRepository.UpdateAsync(registration, ct: ct);
            return true;
        }

        return false;
    }

    public async Task<bool> CancelAsync(Guid studentId, Guid classId, CancellationToken ct = default)
    {
        var registration = await _registrationRepository.FirstOrDefaultAsync(
            r => r.ClassId == classId && r.StudentId == studentId,
            asNoTracking: false,
            ct: ct);

        if (registration is null ||
            string.Equals(registration.RegistrationStatus, StatusCancelled, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        registration.RegistrationStatus = StatusCancelled;
        await _registrationRepository.UpdateAsync(registration, ct: ct);
        return true;
    }

    public async Task<bool> IsRegisteredAsync(Guid studentId, Guid classId, CancellationToken ct = default)
    {
        var registration = await _registrationRepository.FirstOrDefaultAsync(
            r => r.ClassId == classId && r.StudentId == studentId,
            asNoTracking: true,
            ct: ct);

        return registration is not null &&
               string.Equals(registration.RegistrationStatus, StatusActive, StringComparison.OrdinalIgnoreCase);
    }
}

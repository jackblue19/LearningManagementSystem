namespace LMS.Services.Interfaces.StudentService;

public interface IClassRegistrationService
{
    Task<bool> RegisterAsync(Guid studentId, Guid classId, CancellationToken ct = default);
    Task<bool> CancelAsync(Guid studentId, Guid classId, CancellationToken ct = default);
    Task<bool> IsRegisteredAsync(Guid studentId, Guid classId, CancellationToken ct = default);
}

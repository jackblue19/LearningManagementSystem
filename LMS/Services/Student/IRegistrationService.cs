using LMS.Models.Entities;

namespace LMS.Services.Student;

public interface IRegistrationService
{
    Task<bool> CanRegisterAsync(Guid studentId, Guid classId, CancellationToken ct = default);
    Task<ClassRegistration> RegisterAsync(Guid studentId, Guid classId, string? note, CancellationToken ct = default);
}

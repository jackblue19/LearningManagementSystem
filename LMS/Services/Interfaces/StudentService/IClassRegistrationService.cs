using LMS.Models.ViewModels;
using LMS.Models.ViewModels.StudentService;

namespace LMS.Services.Interfaces.StudentService;

public interface IClassRegistrationService
{
    // RegistrationStatus = "approved"
    Task<bool> RegisterAsync(Guid studentId, Guid classId, CancellationToken ct = default);

    // RegistrationStatus = "rejected" (update no delete)
    Task<bool> CancelAsync(Guid studentId, Guid classId, CancellationToken ct = default);

    Task<PagedResult<StudentRegisteredClassVm>> ListMyRegistrationsAsync(
        Guid studentId, int pageIndex = 1, int pageSize = 20, CancellationToken ct = default);
        
    Task<bool> IsRegisteredAsync(Guid studentId, Guid classId, CancellationToken ct = default);
}

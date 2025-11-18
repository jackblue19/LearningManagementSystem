using LMS.Models.ViewModels;
using LMS.Models.ViewModels.StudentService;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentExamService
{
    Task<PagedResult<StudentExamVm>> ListExamsAsync(
           Guid studentId, DateOnly? from = null, DateOnly? to = null, bool upcomingOnly = false,
           int pageIndex = 1, int pageSize = 20, CancellationToken ct = default);
}

using LMS.Models.ViewModels;
using LMS.Models.ViewModels.StudentService;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentExamResultService
{
    Task<PagedResult<StudentExamResultVm>> ListMyResultsAsync(
      Guid studentId, int pageIndex = 1, int pageSize = 20, CancellationToken ct = default);
}

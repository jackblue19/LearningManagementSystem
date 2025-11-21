using LMS.Models.ViewModels;
using LMS.Models.ViewModels.StudentService;

using LMS.Models.Entities;

namespace LMS.Services.Interfaces.StudentService;

public interface IStudentCourseService
{
    // Danh sách lớp chưa đăng ký (lọc theo search/center/subject)
    Task<PagedResult<StudentCourseListItemVm>> ListAvailableClassesAsync(
        Guid studentId, string? search = null, Guid? centerId = null, long? subjectId = null,
        int pageIndex = 1, int pageSize = 20, CancellationToken ct = default);

    // Danh sách lớp đã đăng ký
    Task<PagedResult<StudentCourseListItemVm>> ListMyClassesAsync(
        Guid studentId, int pageIndex = 1, int pageSize = 20, CancellationToken ct = default);
        
    Task<IReadOnlyList<Class>> GetRegisteredClassesAsync(
        Guid studentId,
        bool includeCancelled = false,
        CancellationToken ct = default);
}

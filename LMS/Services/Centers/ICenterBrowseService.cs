using LMS.Models.Entities;
using LMS.Models.ViewModels;

namespace LMS.Services.Centers;

public interface ICenterBrowseService
{
    Task<IReadOnlyList<Center>> ListCentersAsync(CancellationToken ct = default);
    Task<Center?> GetCenterAsync(Guid centerId, CancellationToken ct = default);
    Task<IReadOnlyList<Subject>> ListSubjectsByCenterAsync(Guid centerId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListTeachersAsync(CancellationToken ct = default);

    Task<PagedResult<Class>> PagedClassesAsync(
        Guid centerId,
        long? subjectId,
        Guid? teacherId,
        int pageIndex,
        int pageSize,
        CancellationToken ct = default);
}

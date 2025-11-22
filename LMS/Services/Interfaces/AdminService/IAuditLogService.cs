using LMS.Models.ViewModels;
using LMS.Models.ViewModels.Admin;

namespace LMS.Services.Interfaces.AdminService;

public interface IAuditLogService
{
    Task<PagedResult<AuditLogListItemViewModel>> GetAuditLogsAsync(
        string? searchTerm = null,
        string? actionFilter = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task LogActionAsync(
        Guid userId,
        string actionType,
        string entityName,
        string recordId,
        string? oldData = null,
        string? newData = null,
        CancellationToken ct = default);
}

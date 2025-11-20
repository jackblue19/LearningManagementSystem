using System.Linq;
using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.Admin;
using LMS.Repositories.Interfaces.Communication;
using LMS.Services.Interfaces.AdminService;

namespace LMS.Services.Impl.AdminService;

public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<PagedResult<AuditLogListItemViewModel>> GetAuditLogsAsync(
        string? searchTerm = null,
        string? actionFilter = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;

        var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
        var hasAction = !string.IsNullOrWhiteSpace(actionFilter);
        var hasDateFrom = dateFrom.HasValue;
        var hasDateTo = dateTo.HasValue;

        Expression<Func<AuditLog, bool>>? predicate = null;

        if (hasSearch || hasAction || hasDateFrom || hasDateTo)
        {
            var searchLower = hasSearch ? searchTerm!.ToLower() : null;
            var action = actionFilter;
            var from = dateFrom?.Date;
            var to = dateTo?.Date.AddDays(1); // exclusive upper bound

            predicate = log =>
                (!hasSearch ||
                 log.User.Username.ToLower().Contains(searchLower!) ||
                 (log.User.FullName != null && log.User.FullName.ToLower().Contains(searchLower!)) ||
                 (log.EntityName != null && log.EntityName.ToLower().Contains(searchLower!)) ||
                 (log.RecordId != null && log.RecordId.ToLower().Contains(searchLower!)) ||
                 (log.ActionType != null && log.ActionType.ToLower().Contains(searchLower!))) &&
                (!hasAction || log.ActionType == action) &&
                (!hasDateFrom || log.CreatedAt >= from) &&
                (!hasDateTo || log.CreatedAt < to);
        }

        var includes = new Expression<Func<AuditLog, object>>[]
        {
            log => log.User
        };

        var skip = (pageIndex - 1) * pageSize;

        var items = await _auditLogRepository.ListAsync(
            predicate: predicate,
            orderBy: q => q.OrderByDescending(l => l.CreatedAt),
            skip: skip,
            take: pageSize,
            asNoTracking: true,
            includes: includes,
            ct: ct);

        var total = await _auditLogRepository.CountAsync(predicate, ct);

        var mapped = items.Select(log => new AuditLogListItemViewModel
        {
            LogId = log.LogId,
            Username = log.User.Username,
            FullName = log.User.FullName,
            ActionType = log.ActionType,
            EntityName = log.EntityName,
            RecordId = log.RecordId,
            CreatedAt = log.CreatedAt,
            OldData = log.OldData,
            NewData = log.NewData
        }).ToList();

        return new PagedResult<AuditLogListItemViewModel>(mapped, total, pageIndex, pageSize);
    }
}

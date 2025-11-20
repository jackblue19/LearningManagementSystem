using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.Admin;

namespace LMS.Services.Interfaces.AdminService;

public interface IFeedbackService
{
    Task<PagedResult<FeedbackListItemViewModel>> GetFeedbacksAsync(
        string? searchTerm = null,
        string? statusFilter = null,
        bool? visibilityFilter = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default);

    Task<Feedback?> GetByIdAsync(long feedbackId, CancellationToken ct = default);

    Task UpdateStatusAsync(long feedbackId, string status, CancellationToken ct = default);

    Task UpdateVisibilityAsync(long feedbackId, bool isVisible, CancellationToken ct = default);
}

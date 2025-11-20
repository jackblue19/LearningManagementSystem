using System.Linq;
using System.Linq.Expressions;
using LMS.Models.Entities;
using LMS.Models.ViewModels;
using LMS.Models.ViewModels.Admin;
using LMS.Repositories.Interfaces.Communication;
using LMS.Services.Interfaces.AdminService;

namespace LMS.Services.Impl.AdminService;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;

    public FeedbackService(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<PagedResult<FeedbackListItemViewModel>> GetFeedbacksAsync(
        string? searchTerm = null,
        string? statusFilter = null,
        bool? visibilityFilter = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        if (pageIndex < 1) pageIndex = 1;
        if (pageSize < 1) pageSize = 20;

        var hasSearch = !string.IsNullOrWhiteSpace(searchTerm);
        var hasStatus = !string.IsNullOrWhiteSpace(statusFilter);
        var hasVisibility = visibilityFilter.HasValue;

        Expression<Func<Feedback, bool>>? predicate = null;

        if (hasSearch || hasStatus || hasVisibility)
        {
            var searchLower = hasSearch ? searchTerm!.ToLower() : null;
            var status = statusFilter;
            var isVisible = visibilityFilter;

            predicate = f =>
                (!hasSearch ||
                 (f.Content != null && f.Content.ToLower().Contains(searchLower!)) ||
                 f.User.Username.ToLower().Contains(searchLower!) ||
                 (f.User.FullName != null && f.User.FullName.ToLower().Contains(searchLower!)) ||
                 f.Class.ClassName.ToLower().Contains(searchLower!)) &&
                (!hasStatus || f.FbStatus == status) &&
                (!hasVisibility || (f.IsVisible ?? false) == isVisible!.Value);
        }

        var includes = new Expression<Func<Feedback, object>>[]
        {
            f => f.User,
            f => f.Class
        };

        var skip = (pageIndex - 1) * pageSize;

        var items = await _feedbackRepository.ListAsync(
            predicate: predicate,
            orderBy: q => q.OrderByDescending(f => f.CreatedAt),
            skip: skip,
            take: pageSize,
            asNoTracking: true,
            includes: includes,
            ct: ct);

        var total = await _feedbackRepository.CountAsync(predicate, ct);

        var mapped = items
            .Select(f => new FeedbackListItemViewModel
            {
                FeedbackId = f.FeedbackId,
                Username = f.User.Username,
                FullName = f.User.FullName,
                ClassName = f.Class.ClassName,
                Content = f.Content,
                Rating = f.Rating,
                FbStatus = f.FbStatus,
                IsVisible = f.IsVisible ?? true,
                CreatedAt = f.CreatedAt
            })
            .ToList();

        return new PagedResult<FeedbackListItemViewModel>(mapped, total, pageIndex, pageSize);
    }

    public Task<Feedback?> GetByIdAsync(long feedbackId, CancellationToken ct = default)
        => _feedbackRepository.GetByIdAsync(feedbackId, asNoTracking: true, ct);

    public async Task UpdateStatusAsync(long feedbackId, string status, CancellationToken ct = default)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(feedbackId, asNoTracking: false, ct);
        if (feedback is null)
        {
            throw new InvalidOperationException($"Feedback {feedbackId} not found.");
        }

        feedback.FbStatus = status;
        await _feedbackRepository.UpdateAsync(feedback, saveNow: true, ct);
    }

    public async Task UpdateVisibilityAsync(long feedbackId, bool isVisible, CancellationToken ct = default)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(feedbackId, asNoTracking: false, ct);
        if (feedback is null)
        {
            throw new InvalidOperationException($"Feedback {feedbackId} not found.");
        }

        feedback.IsVisible = isVisible;
        await _feedbackRepository.UpdateAsync(feedback, saveNow: true, ct);
    }
}

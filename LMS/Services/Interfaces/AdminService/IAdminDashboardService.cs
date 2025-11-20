using LMS.Models.ViewModels.Admin;

namespace LMS.Services.Interfaces.AdminService;

public interface IAdminDashboardService
{
    Task<AdminDashboardViewModel> GetDashboardAsync(CancellationToken ct = default);
}


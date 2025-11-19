using LMS.Models.ViewModels.Admin;
using LMS.Services.Interfaces.AdminService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LMS.Pages.Admin.Users;

public class IndexModel : PageModel
{
    private readonly IAdminUserService _adminUserService;

    public IndexModel(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? RoleFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActiveFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageIndex { get; set; } = 1;

    public UserListViewModel ViewModel { get; set; } = null!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var result = await _adminUserService.GetUsersAsync(
            searchTerm: SearchTerm,
            roleFilter: RoleFilter,
            isActiveFilter: IsActiveFilter,
            pageIndex: PageIndex,
            pageSize: 20,
            ct: ct);

        ViewModel = new UserListViewModel
        {
            Users = result,
            SearchTerm = SearchTerm,
            RoleFilter = RoleFilter,
            IsActiveFilter = IsActiveFilter,
            AvailableRoles = new List<string> { "admin", "manager", "teacher", "student" }
        };
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _adminUserService.DeleteUserAsync(id, ct);
            TempData["SuccessMessage"] = "User deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error deleting user: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            await _adminUserService.ToggleUserActiveStatusAsync(id, ct);
            TempData["SuccessMessage"] = "User status updated successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error updating user status: {ex.Message}";
        }

        return RedirectToPage(new { SearchTerm, RoleFilter, IsActiveFilter, PageIndex });
    }
}


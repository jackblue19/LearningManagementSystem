using LMS.Models.Entities;
using LMS.Models.ViewModels;

namespace LMS.Models.ViewModels.Admin;

public class UserListViewModel
{
    public PagedResult<User> Users { get; set; } = null!;
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public bool? IsActiveFilter { get; set; }
    public List<string> AvailableRoles { get; set; } = new() { "admin", "manager", "teacher", "student" };
}


using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels.Admin;

public class UserEditViewModel
{
    [Required]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(120, ErrorMessage = "Email cannot exceed 120 characters")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(256, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 256 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "New Password (leave empty to keep current)")]
    public string? Password { get; set; }

    [StringLength(120, ErrorMessage = "Full name cannot exceed 120 characters")]
    [Display(Name = "Full Name")]
    public string? FullName { get; set; }

    [StringLength(30, ErrorMessage = "Phone cannot exceed 30 characters")]
    [Phone(ErrorMessage = "Invalid phone format")]
    [Display(Name = "Phone")]
    public string? Phone { get; set; }

    [StringLength(40, ErrorMessage = "Role cannot exceed 40 characters")]
    [Display(Name = "Role")]
    public string? RoleDesc { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }

    [StringLength(300, ErrorMessage = "Avatar URL cannot exceed 300 characters")]
    [Display(Name = "Avatar URL")]
    public string? Avatar { get; set; }

    [StringLength(300, ErrorMessage = "Cover Image URL cannot exceed 300 characters")]
    [Display(Name = "Cover Image URL")]
    public string? CoverImageUrl { get; set; }

    public List<string> AvailableRoles { get; set; } = new() { "admin", "manager", "teacher", "student" };
}


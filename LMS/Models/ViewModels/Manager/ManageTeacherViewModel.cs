using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels.Manager;

public class ManageTeacherViewModel
{
    public Guid UserId { get; set; }
    
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-100 ký tự")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(120, ErrorMessage = "Email không được vượt quá 120 ký tự")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(120, ErrorMessage = "Họ tên không được vượt quá 120 ký tự")]
    public string FullName { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(30, ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự")]
    public string? Phone { get; set; }
    
    public string? Avatar { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}

public class CreateTeacherViewModel
{
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3-100 ký tự")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(120, ErrorMessage = "Email không được vượt quá 120 ký tự")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(120, ErrorMessage = "Họ tên không được vượt quá 120 ký tự")]
    public string FullName { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(30, ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự")]
    public string? Phone { get; set; }
}

public class EditTeacherViewModel
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(120, ErrorMessage = "Email không được vượt quá 120 ký tự")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(120, ErrorMessage = "Họ tên không được vượt quá 120 ký tự")]
    public string FullName { get; set; } = string.Empty;
    
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [StringLength(30, ErrorMessage = "Số điện thoại không được vượt quá 30 ký tự")]
    public string? Phone { get; set; }
    
    public bool IsActive { get; set; }
}

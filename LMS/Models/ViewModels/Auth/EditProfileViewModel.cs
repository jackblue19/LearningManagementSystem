using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels.Auth;

public class EditProfileViewModel
{
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }

    [Display(Name = "Ảnh đại diện")]
    public IFormFile? AvatarFile { get; set; }

    public string? CurrentAvatar { get; set; }

    [Display(Name = "Ảnh bìa")]
    public IFormFile? CoverImageFile { get; set; }

    public string? CurrentCoverImage { get; set; }
}

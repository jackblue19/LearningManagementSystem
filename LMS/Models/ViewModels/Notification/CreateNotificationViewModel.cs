using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels.Notification;

public class CreateNotificationViewModel
{
    [Required(ErrorMessage = "Tiêu đề không được để trống")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được quá 200 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nội dung không được để trống")]
    [StringLength(2000, ErrorMessage = "Nội dung không được quá 2000 ký tự")]
    [Display(Name = "Nội dung")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại thông báo không được để trống")]
    [Display(Name = "Loại thông báo")]
    public string Type { get; set; } = "info"; // info, success, warning, danger

    [Required(ErrorMessage = "Đối tượng nhận không được để trống")]
    [Display(Name = "Đối tượng nhận")]
    public string RecipientType { get; set; } = "class"; // class, all-teachers, all-students, specific-users

    // Nếu RecipientType = "class"
    [Display(Name = "Chọn lớp")]
    public Guid? ClassId { get; set; }

    // Nếu RecipientType = "specific-users"
    public List<Guid>? UserIds { get; set; }
}

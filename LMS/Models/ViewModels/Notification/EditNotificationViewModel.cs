using System.ComponentModel.DataAnnotations;

namespace LMS.Models.ViewModels.Notification;

public class EditNotificationViewModel
{
    public long NotificationId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [StringLength(200, ErrorMessage = "Tiêu đề không được quá 200 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập nội dung")]
    [StringLength(2000, ErrorMessage = "Nội dung không được quá 2000 ký tự")]
    [Display(Name = "Nội dung")]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại thông báo")]
    [Display(Name = "Loại thông báo")]
    public string Type { get; set; } = "info";
}

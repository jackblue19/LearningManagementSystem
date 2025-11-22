using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LMS.Helpers;

namespace LMS.Pages._Base;

[Authorize(Roles = "student")]
public abstract class AppPageModel : PageModel
{
    protected Guid CurrentUserId => User.GetUserId();
    protected string CurrentUserName => User.GetFullName();
}

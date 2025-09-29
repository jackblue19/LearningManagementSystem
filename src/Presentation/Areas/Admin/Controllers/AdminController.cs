using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Admin.Controllers;
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

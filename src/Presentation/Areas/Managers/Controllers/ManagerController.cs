using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Managers.Controllers;
public class ManagerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

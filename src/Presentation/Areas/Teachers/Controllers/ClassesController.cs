using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Teachers.Controllers;
public class ClassesController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

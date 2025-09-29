using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.Shared.Controllers;
public class PaymentsController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}

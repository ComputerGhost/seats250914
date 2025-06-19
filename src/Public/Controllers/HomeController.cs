using Microsoft.AspNetCore.Mvc;

namespace Public.Controllers;

[Route("/")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}

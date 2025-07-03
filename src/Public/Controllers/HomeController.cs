using Microsoft.AspNetCore.Mvc;
using Public.Models.ViewModels;

namespace Public.Controllers;

[Route("/")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index([FromQuery] bool renderMap = true)
    {
        return View(new HomeViewModel
        {
            ShouldRenderMap = renderMap,
        });
    }
}

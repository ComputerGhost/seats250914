using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Public.Models;
using Public.Models.ViewModels;

namespace Public.Controllers;

[Route("/")]
public class HomeController : Controller
{
    [HttpGet]
    public IActionResult Index([FromServices] IOptions<Config> config, [FromQuery] bool renderMap = true)
    {
        return View(new HomeViewModel
        {
            OrganizerEmail = config.Value.OrganizerEmail,
            ShouldRenderMap = renderMap,
        });
    }
}

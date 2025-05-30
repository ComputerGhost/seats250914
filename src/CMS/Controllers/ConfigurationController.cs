using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;

[Route("/configuration/")]
public class ConfigurationController : Controller
{
    [HttpGet]
    public IActionResult Edit()
    {
        return View();
    }
}

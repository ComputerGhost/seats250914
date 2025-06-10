using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;

[Authorize]
[Route("/configuration/")]
public class ConfigurationController : Controller
{
    [HttpGet]
    public IActionResult Edit()
    {
        return View();
    }
}

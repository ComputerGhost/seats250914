using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Public.Models;
using Public.Models.ViewModels;

namespace Public.Controllers;

[Route("legal")]
public class LegalController : Controller
{
    [HttpGet("privacy-policy")]
    public IActionResult PrivacyPolicy()
    {
        return View();
    }

    [HttpGet("terms-of-service")]
    public IActionResult TermsOfService([FromServices] IOptions<Config> config)
    {
        return View(new TermsOfServiceViewModel
        {
            OrganizerEmail = config.Value.OrganizerEmail
        });
    }
}

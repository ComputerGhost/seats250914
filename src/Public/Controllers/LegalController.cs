using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Public.Models;
using Public.Models.ViewModels;

namespace Public.Controllers;

[Route("legal")]
public class LegalController(IOptions<Config> config) : Controller
{
    [HttpGet("privacy-policy")]
    [HttpHead("privacy-policy")]
    public IActionResult PrivacyPolicy()
    {
        return View(new PrivacyPolicyViewModel
        {
            OrganizerEmail = config.Value.OrganizerEmail
        });
    }

    [HttpGet("terms-of-service")]
    [HttpHead("terms-of-service")]
    public IActionResult TermsOfService()
    {
        return View(new TermsOfServiceViewModel
        {
            OrganizerEmail = config.Value.OrganizerEmail
        });
    }
}

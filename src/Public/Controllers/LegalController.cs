using Microsoft.AspNetCore.Mvc;

namespace Public.Controllers;

[Route("/legal/")]
public class LegalController : Controller
{
    [HttpGet("privacy-policy")]
    public IActionResult PrivacyPolicy()
    {
        return View();
    }

    [HttpGet("terms-of-service")]
    public IActionResult TermsOfService()
    {
        return View();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;

[Authorize]
[Route("/reservations/")]
public class ReservationsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("new/")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpGet("{seat}/details")]
    public IActionResult Details(string seat)
    {
        return View();
    }
}

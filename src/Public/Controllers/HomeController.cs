using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Public.Controllers;

[Route("/")]
public class HomeController(IMediator mediator) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}

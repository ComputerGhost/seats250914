using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Public.Controllers;

[Route("/")]
public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View();
    }
}

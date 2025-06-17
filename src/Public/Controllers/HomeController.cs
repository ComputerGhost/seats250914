using Core.Application.Seats;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Public.ViewModels;

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

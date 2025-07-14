using CMS.ViewModels;
using Core.Application.Seats;
using Core.Application.System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;

[Authorize]
[Route("/")]
public class HomeController(IMediator mediator) : Controller
{
    [HttpGet]
    [HttpHead]
    public async Task<IActionResult> Index()
    {
        var configuration = await mediator.Send(new FetchConfigurationQuery());
        var seats = await mediator.Send(new ListSeatsQuery());
        return View(new HomeIndexViewModel(configuration, seats));
    }
}

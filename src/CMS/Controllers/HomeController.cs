using CMS.ViewModels;
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
    public async Task<IActionResult> Index()
    {
        var test = await mediator.Send(new TestDatabaseQuery());
        return View(new HomeIndexViewModel
        {
            Success = !test.IsError,
            Error = test.IsError ? test.Errors.FirstOrDefault().Description : null,
        });
    }
}

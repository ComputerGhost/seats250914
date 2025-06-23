using CMS.ViewModels;
using Core.Application.System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;

[Authorize]
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
        var test = await _mediator.Send(new TestDatabaseQuery());
        return View(new TestDatabaseViewModel
        {
            Success = !test.IsError,
            Error = test.IsError ? test.Errors.FirstOrDefault().Description : null,
        });
    }
}

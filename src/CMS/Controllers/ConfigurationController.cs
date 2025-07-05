using CMS.ViewModels;
using Core.Application.System;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;

[Authorize]
[Route("configuration")]
public class ConfigurationController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var result = await mediator.Send(new FetchConfigurationQuery());
        return View(new ConfigurationEditViewModel(result));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ConfigurationEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await mediator.Send(model.ToSaveCommand());
        return View(model.WithSuccessfulSave());
    }
}

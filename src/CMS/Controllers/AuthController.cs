using CMS.Features.Authentication;
using CMS.ViewModels;
using Core.Application.Accounts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CMS.Controllers;

[Route("/auth/")]
public class AuthController(IMediator mediator) : Controller
{
    [HttpGet("setup")]
    public async Task<IActionResult> SetUp([FromServices] IOptions<AuthenticationOptions> authenticationOptions)
    {
        var accounts = await mediator.Send(new ListAccountsQuery());
        if (accounts.Data.Any())
        {
            return StatusCode(StatusCodes.Status423Locked);
        }

        var result = await mediator.Send(new CreateAccountCommand
        {
            IsEnabled = true,
            Login = authenticationOptions.Value.InitialUsername,
            Password = authenticationOptions.Value.InitialPassword,
        });

        if (result.IsError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return RedirectToAction(nameof(SignIn));
    }

    [HttpGet("sign-in")]
    public async Task<IActionResult> SignIn()
    {
        var accounts = await mediator.Send(new ListAccountsQuery());
        if (!accounts.Data.Any())
        {
            return RedirectToAction(nameof(SetUp));
        }

        return View();
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromForm] AccountSignInViewModel model, [FromQuery] string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            model.Password = "";
            return View(model);
        }

        var result = await mediator.Send(new VerifyPasswordCommand
        {
            Login = model.Login,
            Password = model.Password,
        });

        if (result.IsError)
        {
            ModelState.AddModelError("", "The credentials that you entered are incorrect.");
            return View(model);
        }

        var authService = new AuthenticationService(HttpContext);
        await authService.SignIn(model.Login);

        if (Uri.TryCreate(returnUrl, UriKind.Relative, out var validatedReturnUri))
        {
            return Redirect(validatedReturnUri.ToString());
        }
        else
        {
            return Redirect("/");
        }
    }

    [HttpGet("sign-out")]
    public new async Task<IActionResult> SignOut()
    {
        var authService = new AuthenticationService(HttpContext);
        await authService.SignOut();

        return RedirectToAction(nameof(SignIn));
    }
}

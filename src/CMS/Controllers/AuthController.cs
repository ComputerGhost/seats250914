using CMS.ViewModels;
using Core.Application.Accounts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Presentation.Shared.Authentication;
using Serilog;

namespace CMS.Controllers;

[Route("auth")]
public class AuthController(IMediator mediator, IStringLocalizer<AuthController> localizer) : Controller
{
    [HttpGet("setup")]
    public async Task<IActionResult> SetUp([FromServices] IOptions<AuthenticationOptions> authenticationOptions)
    {
        Log.Information("Setting up system.");

        var accounts = await mediator.Send(new ListAccountsQuery());
        if (accounts.Data.Any())
        {
            Log.Error("The system is already set up. Aborting setup.");
            return StatusCode(StatusCodes.Status423Locked);
        }

        var result = await mediator.Send(new CreateAccountCommand
        {
            IsEnabled = true,
            Login = authenticationOptions.Value.InitialUsername,
            Password = authenticationOptions.Value.InitialPassword,
        });

        return result.Match<IActionResult>(
            result => RedirectToAction(nameof(SignIn)),
            errors => StatusCode(StatusCodes.Status500InternalServerError));
    }

    [HttpGet("sign-in")]
    public async Task<IActionResult> SignIn()
    {
        Log.Information("Verifying that there are existing accounts.");
        var accounts = await mediator.Send(new ListAccountsQuery());
        if (!accounts.Data.Any())
        {
            Log.Information("Redirecting to setup endpoint.");
            return RedirectToAction(nameof(SetUp));
        }

        return View();
    }

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn([FromForm] AuthSignInViewModel model, [FromQuery] string? returnUrl)
    {
        var result = await mediator.Send(new VerifyPasswordCommand
        {
            Login = model.Login,
            Password = model.Password,
        });

        return await result.MatchAsync(
            async result => await SuccessfulSignIn(),
            errors => IncorrectCredentials());

        Task<IActionResult> IncorrectCredentials()
        {
            ModelState.AddModelError("", localizer["IncorrectCredentials"]);
            return Task.FromResult<IActionResult>(View(model));
        }

        async Task<IActionResult> SuccessfulSignIn()
        {
            var authService = new AuthenticationService(HttpContext);
            await authService.SignIn(model.Login);

            var safeReturnUrl = Uri.TryCreate(returnUrl, UriKind.Relative, out var validatedReturnUri)
                ? validatedReturnUri.ToString() : "/";
            return Redirect(safeReturnUrl);
        }
    }

    [Authorize]
    [HttpPost("sign-out")]
    public new async Task<IActionResult> SignOut()
    {
        Log.Information("Signing out user {Name}.", User.Identity?.Name);

        var authService = new AuthenticationService(HttpContext);
        await authService.SignOut();

        return RedirectToAction(nameof(SignIn));
    }
}

using CMS.ViewModels;
using Core.Application.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace CMS.Controllers;

[Authorize]
[Route("/accounts/")]
public class AccountsController(IMediator mediator, IStringLocalizer<AccountsController> localizer) : Controller
{
    [HttpGet("new")]
    public IActionResult Create()
    {
        return View(new AccountCreateViewModel());
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create([FromForm] AccountCreateViewModel model)
    {
        var result = await mediator.Send(new CreateAccountCommand
        {
            Login = model.Login,
            Password = model.Password,
            IsEnabled = model.IsEnabled,
        });

        return result.Match(
            result => RedirectToAction(nameof(Details), new { model.Login }),
            errors => errors.First().Type switch
            {
                ErrorType.Conflict => LoginConflict(),
                ErrorType.NotFound => NotFound(),
                _ => throw new NotImplementedException(),
            });

        IActionResult LoginConflict()
        {
            ModelState.AddModelError(nameof(AccountCreateViewModel.Login), localizer["Login conflict"]);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accounts = await mediator.Send(new ListAccountsQuery());
        return View(accounts);
    }

    [HttpGet("{login}/details")]
    public async Task<IActionResult> Details(string login)
    {
        var result = await mediator.Send(new FetchAccountQuery(login));
        return result.Match<IActionResult>(
            result => View(new AccountViewViewModel(result)),
            errors => errors.First().Type switch
            {
                ErrorType.NotFound => NotFound(),
                _ => throw new NotImplementedException(),
            });
    }

    [HttpGet("{login}/edit")]
    public async Task<IActionResult> Edit(string login)
    {
        var result = await mediator.Send(new FetchAccountQuery(login));
        return result.Match<IActionResult>(
            result => View(new AccountEditViewModel(result)),
            errors => errors.First().Type switch
            {
                ErrorType.NotFound => NotFound(),
                _ => throw new NotImplementedException(),
            });
    }

    [HttpPost("{login}/edit")]
    public async Task<IActionResult> Edit(string login, [FromForm] AccountEditViewModel model)
    {
        return model.Action switch
        {
            "UpdateAccount" => await UpdateAccount(),
            "ChangePassword" => await ChangePassword(),
            _ => BadRequest($"The form property `Action` has the invalid value '{model.Action}'.")
        };

        async Task<IActionResult> ChangePassword()
        {
            var result = await mediator.Send(new UpdatePasswordCommand
            {
                Login = login,
                Password = model.Password,
            });

            return result.Match<IActionResult>(
                result => View(model.WithSuccessfulPasswordChange()),
                errors => errors.First().Type switch
                {
                    ErrorType.NotFound => NotFound(),
                    _ => throw new NotImplementedException(),
                });
        }

        async Task<IActionResult> UpdateAccount()
        {
            var result = await mediator.Send(new UpdateAccountCommand
            {
                Login = login,
                IsEnabled = model.IsEnabled,
            });

            return result.Match<IActionResult>(
                result => RedirectToAction(nameof(Details), new { login }),
                errors => errors.First().Type switch
                {
                    ErrorType.NotFound => NotFound(),
                    _ => throw new NotImplementedException(),
                });
        }
    }
}

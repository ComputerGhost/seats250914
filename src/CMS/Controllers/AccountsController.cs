using CMS.ViewModels;
using Core.Application.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;

[Authorize]
[Route("/accounts/")]
public class AccountsController(IMediator mediator) : Controller
{
    [HttpGet("new")]
    public IActionResult Create()
    {
        var model = new AccountCreateViewModel();
        return View(model);
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create([FromForm] AccountCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await mediator.Send(new CreateAccountCommand
        {
            Login = model.Login,
            Password = model.Password,
            IsEnabled = model.IsEnabled,
        });

        return CheckForError(result) ?? RedirectToAction(nameof(Details), new { model.Login });
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
        return CheckForError(result) ?? View(new AccountViewViewModel(result.Value));
    }

    [HttpGet("{login}/edit")]
    public async Task<IActionResult> Edit(string login)
    {
        var result = await mediator.Send(new FetchAccountQuery(login));
        return CheckForError(result) ?? View(new AccountEditViewModel(result.Value));
    }

    [HttpPost("{login}/edit")]
    public async Task<IActionResult> Edit(string login, [FromForm] AccountEditViewModel model)
    {
        if (login != model.Login)
        {
            return BadRequest("The route parameter `login` and the view model's `Login` are not equal.");
        }

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

            model.IsPasswordChangeSuccessful = !result.IsError;

            return CheckForError(result) ?? View(model);
        }

        async Task<IActionResult> UpdateAccount()
        {
            var result = await mediator.Send(new UpdateAccountCommand
            {
                Login = login,
                IsEnabled = model.IsEnabled,
            });

            return CheckForError(result) ?? RedirectToAction(nameof(Details), new { Login = login });
        }
    }

    private IActionResult? CheckForError(IErrorOr errorOr)
    {
        if (!errorOr.IsError)
        {
            return null;
        }

        var firstError = errorOr.Errors![0];

        if (firstError.Type == ErrorType.NotFound)
        {
            return NotFound();
        }
        else if (firstError.Type == ErrorType.Conflict)
        {
            ModelState.AddModelError(nameof(AccountCreateViewModel.Login), firstError.Description);
            return null;
        }
        else
        {
            throw new Exception(firstError.Description);
        }
    }
}

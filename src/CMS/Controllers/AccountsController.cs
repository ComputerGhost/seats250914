using CMS.ViewModels;
using Core.Application.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CMS.Controllers;

[Route("/accounts/")]
public class AccountsController : Controller
{
    private readonly IMediator _mediator;

    public AccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{login}/change-password")]
    public async Task<IActionResult> ChangePassword(string login, [FromBody] string password)
    {
        var result = await _mediator.Send(new UpdatePasswordCommand {
            Login = login,
            Password = password
        });
        return result.IsError ? NotFound() : Ok();
    }

    [HttpGet("new")]
    public IActionResult Create()
    {
        var model = new AccountCreateViewModel();
        return View(model);
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(AccountCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _mediator.Send(new CreateAccountCommand
        {
            Login = model.Login,
            Password = model.Password,
            IsEnabled = model.IsEnabled,
        });

        if (result.IsError && result.FirstError.Type == ErrorType.Conflict)
        {
            ModelState.AddModelError(nameof(AccountCreateViewModel.Login), $"The account '{model.Login}' already exists.");
            return View(model);
        }

        return RedirectToAction(nameof(Details), new { model.Login });
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accounts = await _mediator.Send(new ListAccountsQuery());
        return View(accounts);
    }

    [HttpGet("{login}/details")]
    public async Task<IActionResult> Details(string login)
    {
        var result = await _mediator.Send(new FetchAccountQuery(login));
        if (result.IsError && result.FirstError.Type == ErrorType.NotFound)
        {
            return NotFound();
        }

        return View(new ViewAccountViewModel(result.Value));
    }

    [HttpGet("{login}/edit")]
    public async Task<IActionResult> Edit(string login)
    {
        var result = await _mediator.Send(new FetchAccountQuery(login));
        if (result.IsError && result.FirstError.Type == ErrorType.NotFound)
        {
            return NotFound();
        }

        return View(new ViewAccountViewModel(result.Value));
    }

    [HttpPost("{login}/edit")]
    public async Task<IActionResult> Edit(string login, UpdateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsError && result.FirstError.Type == ErrorType.NotFound)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { Login = login });
    }

    [HttpGet("sign-in")]
    public IActionResult SignIn()
    {
        return View();
    }

    [HttpPost("sign-in")]
    public IActionResult SignIn([FromBody] string username, [FromBody] string password)
    {
        return Redirect("/");
    }

    [HttpGet("sign-out")]
    public new IActionResult SignOut()
    {
        return RedirectToAction(nameof(SignIn));
    }
}

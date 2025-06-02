using CMS.ViewModels;
using Core.Application.Accounts;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
        return View();
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.IsError && result.FirstError.Type == ErrorType.Conflict)
        {
            ModelState.AddModelError(nameof(CreateAccountCommand.Login), $"The account '{command.Login}' already exists.");
            return View(command);
        }

        return RedirectToAction(nameof(Details), new { command.Login });
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

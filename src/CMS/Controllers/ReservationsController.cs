using CMS.Extensions;
using CMS.ViewModels;
using Core.Application.Reservations;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMS.Controllers;

[Authorize]
[Route("/reservations/")]
public class ReservationsController(IMediator mediator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var reservations = await mediator.Send(new ListReservationsQuery());
        return View(new ReservationListViewModel(reservations));
    }

    [HttpGet("new")]
    public IActionResult Create()
    {
        var model = new ReservationCreateViewModel();
        return View(model);
    }

    [HttpPost("new")]
    public async Task<IActionResult> Create(ReservationCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var ipAddress = Request.GetClientIpAddress();

        var lockResult = await mediator.Send(model.ToLockSeatCommand(ipAddress));
        if (lockResult.IsError)
        {
            ModelState.AddModelError(nameof(ReservationCreateViewModel.SeatNumber), "Could not lock seat.");
            return View(model);
        }

        var reserveResult = await mediator.Send(model.ToReserveSeatCommand(ipAddress, lockResult.Value));
        return CheckForError(reserveResult)
            ?? RedirectToAction(nameof(Details), new { reservationId = reserveResult.Value });
    }

    [HttpGet("{reservationId}/details")]
    public async Task<IActionResult> Details(int reservationId)
    {
        var result = await mediator.Send(new FetchReservationQuery(reservationId));
        return CheckForError(result) ?? View(new ReservationViewViewModel(result.Value));
    }

    [HttpPost("{reservationId}/details")]
    public async Task<IActionResult> Details(int reservationId, [FromForm] string action)
    {
        var result = action switch
        {
            "approve" => await ApproveReservation(),
            "reject" => await RejectReservation(),
            _ => throw new NotImplementedException(),
        };

        return CheckForError(result) ?? RedirectToAction("Details", new { reservationId });

        Task<ErrorOr<Success>> ApproveReservation()
        {
            var command = new ApproveReservationCommand(reservationId);
            return mediator.Send(command);
        }

        Task<ErrorOr<Success>> RejectReservation()
        {
            var command = new RejectReservationCommand(reservationId);
            return mediator.Send(command);
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

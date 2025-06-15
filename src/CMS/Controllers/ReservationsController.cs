using CMS.Extensions;
using CMS.ViewModels;
using Core.Application.Reservations;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Diagnostics;

namespace CMS.Controllers;

[Authorize]
[Route("/reservations/")]
public class ReservationsController(IMediator mediator, IStringLocalizer<ReservationsController> localizer) : Controller
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
        var ipAddress = Request.GetClientIpAddress();

        var lockResult = await mediator.Send(model.ToLockSeatCommand(ipAddress));
        if (lockResult.IsError)
        {
            return SeatConflict();
        }

        var reserveResult = await mediator.Send(model.ToReserveSeatCommand(ipAddress, lockResult.Value));
        Debug.Assert(!reserveResult.IsError, "Reserving a seat should not be unsuccessful here.");
        return RedirectToAction(nameof(Details), new { reservationId = reserveResult.Value });

        IActionResult SeatConflict()
        {
            ModelState.AddModelError(nameof(ReservationCreateViewModel.SeatNumber), localizer["Could not lock seat."]);
            return View(model);
        }
    }

    [HttpGet("{reservationId}/details")]
    public async Task<IActionResult> Details(int reservationId)
    {
        var result = await mediator.Send(new FetchReservationQuery(reservationId));
        return result.Match<IActionResult>(
            result => View(new ReservationViewViewModel(result)),
            errors => errors.First().Type switch
            {
                ErrorType.NotFound => NotFound(),
                _ => throw new NotImplementedException(),
            });
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

        return result.Match<IActionResult>(
            result => RedirectToAction(nameof(Details), new { reservationId }),
            errors => errors.First().Type switch
            {
                ErrorType.NotFound => NotFound(),
                _ => throw new NotImplementedException(),
            });

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
}

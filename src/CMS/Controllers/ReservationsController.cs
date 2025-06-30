using CMS.ViewModels;
using Core.Application.Reservations;
using Core.Application.Seats;
using Core.Domain.Common.Enumerations;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Presentation.Shared.FrameworkEnhancements.Extensions;
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
    public async Task<IActionResult> Create()
    {
        var activeSeats = await mediator.Send(new ListSeatsQuery
        {
            StatusFilter = SeatStatus.Available,
        });
        var model = new ReservationCreateViewModel(activeSeats);
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
            result => View(new ReservationViewViewModel(reservationId, result)),
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

    [HttpGet("{reservationId}/edit")]
    public async Task<IActionResult> Edit(int reservationId)
    {
        var result = await mediator.Send(new FetchReservationQuery(reservationId));
        return result.Match<IActionResult>(
            result => View(new ReservationEditViewModel(result)),
            errors => errors.First().Type switch
            {
                ErrorType.NotFound => NotFound(),
                _ => throw new NotImplementedException(),
            });
    }

    [HttpPost("{reservationId}/edit")]
    public async Task<IActionResult> Edit(int reservationId, [FromForm] ReservationEditViewModel model)
    {
        var result = await mediator.Send(model.ToUpdateReservationCommand(reservationId));

        return result.Match<IActionResult>(
            result => RedirectToAction(nameof(Details), new { reservationId }),
            errors => errors.First().Type switch
            {
                ErrorType.NotFound => NotFound(),
                _ => throw new NotImplementedException(),
            });
    }
}

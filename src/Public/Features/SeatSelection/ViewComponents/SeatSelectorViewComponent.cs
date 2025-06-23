using Core.Application.Seats;
using Core.Application.System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Public.Features.SeatSelection.Models;

namespace Public.Features.SeatSelection.ViewComponents;

/// <summary>
/// Widget to select a seat
/// </summary>
/// <remarks>
/// See Views/Shared/Components/SeatSelector for the associated view.
/// </remarks>
public class SeatSelectorViewComponent(IMediator mediator) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(string idPrefix)
    {
        var seatsList = await mediator.Send(new ListSeatsQuery());
        var systemStatus = await mediator.Send(new FetchReservationsStatusQuery());
        return View(new SeatSelectorViewModel(seatsList, systemStatus)
        {
            IdPrefix = idPrefix,
            UrlForLockSeat = Url.Action("LockSeat", "Api")!,
            UrlForReservationPage = Url.Action("ReserveSeat", "Reservation")!,
        });
    }
}

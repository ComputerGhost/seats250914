using Core.Application.Seats;
using Core.Application.System;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Public.Models.ViewModels;

namespace Public.ViewComponents;

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
            UrlForLockSeat = Url.Action("LockSeats", "Api")!,
            UrlForReservationPage = Url.Action("ReserveSeat", "Reservation")!,
        });
    }
}

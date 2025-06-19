using Core.Application.Seats;
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
        var listSeatsResult = await mediator.Send(new ListSeatsQuery());
        return View(new SeatSelectorViewModel(listSeatsResult)
        {
            IdPrefix = idPrefix,
            LockSeatUrl = Url.Action("LockSeat", "Api")!,
            ReservationPageUrl = Url.Action("ReserveSeat", "Reservation")!,
        });
    }
}

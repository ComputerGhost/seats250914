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
    public IViewComponentResult Invoke2()
    {
        return View();
    }

    public async Task<IViewComponentResult> InvokeAsync(int? selectedSeat)
    {
        var result = await mediator.Send(new ListSeatsQuery());

        var model = new SeatSelectorViewModel(result)
        {
            SelectedSeat = selectedSeat,
        };

        return View(model);
    }
}

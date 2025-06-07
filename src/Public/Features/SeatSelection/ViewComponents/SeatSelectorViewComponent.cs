using Microsoft.AspNetCore.Mvc;

namespace Public.Features.SeatSelection.ViewComponents;

/// <summary>
/// Widget to select a seat
/// </summary>
/// <remarks>
/// See Views/Shared/Components/SeatSelector for the associated view.
/// </remarks>
public class SeatSelectorViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        return View();
    }
}

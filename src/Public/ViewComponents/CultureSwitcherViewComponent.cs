using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Presentation.Shared.Localization.Extensions;
using Presentation.Shared.Localization.Models;

namespace Public.ViewComponents;

/// <summary>
/// Dropdown menu to select a culture.
/// </summary>
/// <remarks>
/// See Views/Shared/Components/CultureSwitcher for the associated view.
/// </remarks>
public class CultureSwitcherViewComponent : ViewComponent
{
    private readonly LocalizationOptions _localizationOptions;

    public CultureSwitcherViewComponent(IOptions<LocalizationOptions> options)
    {
        _localizationOptions = options.Value;
    }

    public IViewComponentResult Invoke()
    {
        // Supported UI cultures, same value as `RequestLocalizationOptions.SupportedUICultures`.
        var supportedCultures = _localizationOptions.SupportedCultures;

        var model = new CultureSwitcherModel
        {
            SupportedCultures = supportedCultures,
            CurrentUICulture = HttpContext.GetRequestCulture(),
        };

        return View(model);
    }
}

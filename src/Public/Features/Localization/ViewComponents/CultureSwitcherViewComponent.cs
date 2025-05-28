using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Public.Features.Localization.Extensions;
using Public.Features.Localization.Models;

namespace Public.Features.Localization.ViewComponents;

/// <summary>
/// Dropdown menu to select a culture.
/// </summary>
/// <remarks>
/// See Views/Shared/Components/CultureSwitcher for associated view.
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
            UseRouteCulture = _localizationOptions.UseRouteCulture,
        };

        return View(model);
    }
}

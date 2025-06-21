using System.Globalization;

namespace Presentation.Shared.Localization.Models;

public class CultureSwitcherModel
{
    public const string CULTURE_COOKIE_NAME = ".AspNetCore.Culture";

    public CultureInfo CurrentUICulture { get; set; } = null!;

    public IList<CultureInfo> SupportedCultures { get; set; } = null!;

    public string GetCookieValue(string culture)
    {
        return $"c={culture}|uic={culture}";
    }
}

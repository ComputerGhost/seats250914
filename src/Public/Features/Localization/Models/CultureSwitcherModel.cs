using System.Globalization;

namespace Public.Features.Localization.Models;

public class CultureSwitcherModel
{
    public CultureInfo CurrentUICulture { get; set; } = null!;
    public IList<CultureInfo> SupportedCultures { get; set; } = null!;
    public bool UseRouteCulture { get; set; } = false;
}

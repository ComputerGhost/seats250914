using System.Globalization;

namespace Public.Features.Localization.Models;

public class LocalizationOptions
{
    public IList<CultureInfo> SupportedCultures { get; set; } = null!;

    public CultureInfo DefaultCulture { get; set; } = null!;
}

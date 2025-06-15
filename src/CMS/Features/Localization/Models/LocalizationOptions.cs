using System.Globalization;

namespace CMS.Features.Localization.Models;

public class LocalizationOptions
{
    public IList<CultureInfo> SupportedCultures { get; set; } = null!;

    public CultureInfo DefaultCulture { get; set; } = null!;
}

using System.Globalization;

namespace Public.Features.Localization.Models;

public class LocalizationOptions
{
    public IList<CultureInfo> SupportedCultures { get; set; } = null!;

    public CultureInfo DefaultCulture { get; set; } = null!;

    /// <summary>
    /// Enables the use of resource (.resx) files for localizers to use for translations.
    /// </summary>
    /// <remarks>
    /// The resource files should be placed in the "Resources" folder.  The 
    /// internals of that folder should mirror the project folder structure.
    /// </remarks>
    public bool UseLocalizationResources { get; set; } = false;

    /// <summary>
    /// Prioritizes the `culture` parameter from the route template for determining the culture.
    /// </summary>
    public bool UseRouteCulture { get; set; } = false;

    /// <summary>
    /// Enables entire view localization by having variants use the i18n code as an extension.
    /// </summary>
    /// <example>
    /// A view named Index.cshtml may have a translated version named Index.ko.cshtml.
    /// </example>
    public bool UseViewLocalization { get; set; } = false;
}

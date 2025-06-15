using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using CMS.Features.Localization.Models;

namespace CMS.Features.Localization.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseMyLocalization(this WebApplication app)
    {
        var myOptions = app.Services.GetService<IOptions<LocalizationOptions>>()?.Value;
        if (myOptions == null)
        {
            var message = "`AddMyLocalization` needs to be called before `UseMyLocalization`.";
            throw new InvalidOperationException(message);
        }

        // We'll need a different set of options to pass to the framework.
        var netOptions = new RequestLocalizationOptions
        {
            DefaultRequestCulture = new RequestCulture(myOptions.DefaultCulture),

            // In almost every situation, these should be set to the same thing.
            SupportedCultures = myOptions.SupportedCultures, // Parsing and rendering data like numbers and currency
            SupportedUICultures = myOptions.SupportedCultures, // Language to use
        };

        app.UseRequestLocalization(netOptions);

        return app;
    }
}

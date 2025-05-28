using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Public.Features.Localization.Models;

namespace Public.Features.Localization.Extensions;

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

        if (myOptions.UseLocalizationResources)
        {
            // Nothing to do here.
            // We only needed this in the service extensions.
        }

        if (myOptions.UseRouteCulture)
        {
            netOptions.RequestCultureProviders.Insert(0, new RouteDataRequestCultureProvider());
        }

        app.UseRequestLocalization(netOptions);

        return app;
    }
}

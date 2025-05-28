using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace Public.Features.Localization.Extensions;

public static class HttpContextExtensions
{
    public static CultureInfo GetRequestCulture(this HttpContext context)
    {
        var cultureFeature = context.Features.Get<IRequestCultureFeature>();
        if (cultureFeature == null)
        {
            var message = "`UseMyLocalization`, which calls the needed `UseRequestLocalization`, needs to be called at startup.";
            throw new InvalidOperationException(message);
        }

        return cultureFeature.RequestCulture.UICulture;
    }
}

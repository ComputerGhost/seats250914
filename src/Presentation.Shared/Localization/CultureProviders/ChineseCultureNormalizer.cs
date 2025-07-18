using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace Presentation.Shared.Localization.CultureProviders;

/// <summary>
/// This provider normalizes Chinese culture codes in the `Accept-Languages` header.
/// </summary>
internal class ChineseCultureNormalizer : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var header = httpContext.Request.Headers["Accept-Language"].ToString();

        if (header.StartsWith("zh-CN", StringComparison.OrdinalIgnoreCase) ||
            header.StartsWith("zh-SG", StringComparison.OrdinalIgnoreCase) ||
            header.Equals("zh", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<ProviderCultureResult?>(new("zh-Hans"));
        }

        // Normalization to "zh-Hant" is omitted because the website doesn't support that.
        // Such functionality can be added here if ever needed.

        return Task.FromResult<ProviderCultureResult?>(null);
    }
}

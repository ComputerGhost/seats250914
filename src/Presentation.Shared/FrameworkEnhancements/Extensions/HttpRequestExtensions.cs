using Microsoft.AspNetCore.Http;
using System.Net;

namespace Presentation.Shared.FrameworkEnhancements.Extensions;

public static class HttpRequestExtensions
{
    /// <summary>
    /// Gets the IPv4 address of the client.
    /// </summary>
    /// <remarks>
    /// This is intended to be used with logic to throttle a connection.
    /// It is not sufficient to identify a user by their IP address,
    /// because there are edge cases in which it is not accurate.
    /// </remarks>
    public static string GetClientIpAddress(this HttpRequest request)
    {
        var ipAddress = request.GetXForwardedFor()
            // This should never be null when using TCP.
            ?? request.HttpContext.Connection.RemoteIpAddress
            // But just in case, we fall back to this.
            ?? IPAddress.Loopback;
        return ipAddress.MapToIPv4().ToString();
    }

    private static IPAddress? GetXForwardedFor(this HttpRequest request)
    {
        var headerValue = request.Headers["X-Forwarded-For"].FirstOrDefault();
        var firstValue = headerValue?.Split(',').First();
        return IPAddress.TryParse(firstValue, out var ip) ? ip : null;
    }
}

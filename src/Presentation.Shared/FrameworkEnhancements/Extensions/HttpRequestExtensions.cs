using Microsoft.AspNetCore.Http;
using System.Diagnostics;
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
        var cloudflareForwardedFor = request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (cloudflareForwardedFor != null)
        {
            return cloudflareForwardedFor;
        }

        var xForwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (xForwardedFor != null)
        {
            var firstInChain = xForwardedFor.Split(',').First();
            if (IPAddress.TryParse(firstInChain, out var ip))
            {
                return ip.ToString();
            }
        }

        var remoteIp = request.HttpContext.Connection.RemoteIpAddress;
        Debug.Assert(remoteIp != null, "RemoteIpAddress should not be null when using TCP.");
        return remoteIp.MapToIPv4().ToString();
    }
}

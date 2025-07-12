namespace Presentation.Shared.FrameworkEnhancements.Extensions;
public static class DateTimeOffsetExtensions
{
    public static string ToNormalizedString(this DateTimeOffset when, string timeZone = "Korea Standard Time")
    {
        // My local computer uses "Korea Standard Time", but the server uses "Asia/Seoul".
        if (timeZone is "Korea Standard Time" or "Asia/Seoul")
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            var localTime = TimeZoneInfo.ConvertTime(when, tz);
            return localTime.ToString("yyyy-MM-dd HH:mm:ss") + " (KST)";
        }

        return when.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + " (UTC)";
    }
}

namespace Presentation.Shared.FrameworkEnhancements.Extensions;
public static class DateTimeOffsetExtensions
{
    public static string ToNormalizedString(this DateTimeOffset when, string timeZone = "Korea Standard Time")
    {
        if (timeZone == "Korea Standard Time")
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            var localTime = TimeZoneInfo.ConvertTime(when, tz);
            return localTime.ToString("yyyy-MM-dd HH:mm:ss") + " (KST)";
        }

        return when.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss") + " (UTC)";
    }
}

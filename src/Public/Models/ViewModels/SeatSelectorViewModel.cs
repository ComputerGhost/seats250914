using Core.Application.Seats;
using Core.Application.System;
using Core.Domain.Scheduling;
using Public.Extensions;
using Public.Models.Enumerations;

namespace Public.Models.ViewModels;

public class SeatSelectorViewModel
{
    public SeatSelectorViewModel(ListSeatsQueryResponse seatsList, FetchReservationsStatusQueryResponse systemStatus)
    {
        SeatStatuses = seatsList.Data.ToDictionary(
            seat => seat.SeatNumber,
            seat => seat.Status.ToCssClass());

        SystemStatus = systemStatus.Status switch
        {
            ReservationsStatus.OpeningLater => SystemStatus.OpeningSoon,
            ReservationsStatus.OpenedManually => SystemStatus.Open,
            ReservationsStatus.OpenedPerSchedule => SystemStatus.Open,
            _ => Enum.Parse<SystemStatus>(systemStatus.Status.ToString())
        };

        CloseTimeDisplay = FormatForDisplay(systemStatus.ScheduledCloseDateTime, systemStatus.ScheduledCloseTimeZone);
        CloseTimeParameter = systemStatus.ScheduledCloseDateTime.ToString("s");
        CloseTimeZone = systemStatus.ScheduledCloseTimeZone;
        OpenTimeDisplay = FormatForDisplay(systemStatus.ScheduledOpenDateTime, systemStatus.ScheduledOpenTimeZone);
        OpenTimeParameter = systemStatus.ScheduledOpenDateTime.ToString("s");
        OpenTimeZone = systemStatus.ScheduledOpenTimeZone;
    }

    public required string UrlForReservationPage { get; init; }
    public required string UrlForLockSeat { get; init; }
    public required string IdPrefix { get; init; }

    public string CloseTimeDisplay { get; init; }
    public string CloseTimeParameter { get; init; }
    public string CloseTimeZone { get; init; }
    public string OpenTimeDisplay { get; init; }
    public string OpenTimeParameter { get; init; }
    public string OpenTimeZone { get; init; }

    public IDictionary<int, string> SeatStatuses { get; init; } = new Dictionary<int, string>();
    public SystemStatus SystemStatus { get; init; }
    public bool IsOpen => SystemStatus == SystemStatus.Open;

    private static string FormatForDisplay(DateTimeOffset when, string timeZone)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
        var localTime = TimeZoneInfo.ConvertTime(when, tz);
        var offset = tz.GetUtcOffset(localTime);

        var timeDisplay = localTime.ToString("yyyy-MM-dd HH:mm");

        var offsetDisplay = offset.TotalSeconds switch
        {
            < 0 => "UTC-" + offset.ToString(@"hh\:mm"),
            > 0 => "UTC+" + offset.ToString(@"hh\:mm"),
            _ => "UTC",
        };

        return $"{timeDisplay} {offsetDisplay}";
    }
}

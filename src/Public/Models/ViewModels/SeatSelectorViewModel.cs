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
        CloseTimeParameter = FormatForParameter(systemStatus.ScheduledCloseDateTime);
        CloseTimeZone = systemStatus.ScheduledCloseTimeZone;
        OpenTimeDisplay = FormatForDisplay(systemStatus.ScheduledOpenDateTime, systemStatus.ScheduledOpenTimeZone);
        OpenTimeParameter = FormatForParameter(systemStatus.ScheduledOpenDateTime);
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

        var timeFormat = "yyyy-MM-dd HH:mm";
        var offsetFormat = (offset < TimeSpan.Zero ? @"\-" : "") + @"hh\:mm";
        return $"{localTime.ToString(timeFormat)} UTC{offset.ToString(offsetFormat)}";
    }

    private static string FormatForParameter(DateTimeOffset when)
    {
        return when.ToString("yyyy-MM-ddTHH:mm");
    }
}

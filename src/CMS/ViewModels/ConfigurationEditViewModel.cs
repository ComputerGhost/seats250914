using Core.Application.System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class ConfigurationEditViewModel
{
    // These are options for `ScheduleOverride`.
    public const string OVERRIDE_OPEN = "override-open";
    public const string OVERRIDE_CLOSE = "override-close";
    public const string OVERRIDE_NONE = "override-none";

    public ConfigurationEditViewModel()
    {
    }

    public ConfigurationEditViewModel(FetchConfigurationQueryResponse queryResponse)
    {
        GracePeriodSeconds = queryResponse.GracePeriodSeconds;
        MaxSeatsPerPerson = queryResponse.MaxSeatsPerPerson;
        MaxSeatsPerIPAddress = queryResponse.MaxSeatsPerIPAddress;
        MaxSecondsToConfirmSeat = queryResponse.MaxSecondsToConfirmSeat;
        ScheduledOpenDate = queryResponse.ScheduledOpenDateTime.DateTime;
        ScheduledOpenTime = queryResponse.ScheduledOpenDateTime.DateTime;
        ScheduledOpenTimeZone = queryResponse.ScheduledOpenTimeZone;

        ScheduleOverride = queryResponse.ForceOpenReservations? OVERRIDE_OPEN
            : queryResponse.ForceCloseReservations? OVERRIDE_CLOSE
            : OVERRIDE_NONE;
    }

    /* Form options */

    public IEnumerable<SelectListItem> ValidTimeZones => TimeZoneInfo.GetSystemTimeZones()
        .Select(tz => new SelectListItem(tz.DisplayName, tz.Id));

    /* Form fields */

    public int GracePeriodSeconds { get; set; }

    public int MaxSeatsPerPerson { get; set; }

    public int MaxSeatsPerIPAddress { get; set; }

    public int MaxSecondsToConfirmSeat { get; set; }

    [DataType(DataType.Date)]
    public DateTime ScheduledOpenDate { get; set; }

    [DataType(DataType.Time)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm:ss}")]
    public DateTime ScheduledOpenTime { get; set; }

    /// <summary>
    /// Time zone of scheduled open date. For options see <see cref="ValidTimeZones"/>.
    /// </summary>
    public string ScheduledOpenTimeZone { get; set; } = null!;

    /// <summary>
    /// Whether and how to override schedule.
    /// </summary>
    public string ScheduleOverride { get; set; } = null!;

    public SaveConfigurationCommand ToSaveCommand()
    {
        var scheduledOpenDateTime = GetDateTimeOffset(ScheduledOpenDate, ScheduledOpenTime, ScheduledOpenTimeZone);

        return new SaveConfigurationCommand
        {
            ForceCloseReservations = ScheduleOverride == OVERRIDE_CLOSE,
            ForceOpenReservations = ScheduleOverride == OVERRIDE_OPEN,
            GracePeriodSeconds = GracePeriodSeconds,
            MaxSeatsPerPerson = MaxSeatsPerPerson,
            MaxSeatsPerIPAddress = MaxSeatsPerIPAddress,
            MaxSecondsToConfirmSeat = MaxSecondsToConfirmSeat,
            ScheduledOpenDateTime = scheduledOpenDateTime,
            ScheduledOpenTimeZone = ScheduledOpenTimeZone,
        };
    }

    private static DateTimeOffset GetDateTimeOffset(DateTime dateSource, DateTime timeSource, string timeZoneId)
    {
        var localDateTime = dateSource.Date.Add(timeSource.TimeOfDay);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return new DateTimeOffset(localDateTime, timeZone.BaseUtcOffset);
    }
}

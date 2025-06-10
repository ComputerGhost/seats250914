namespace Core.Domain.Common.Models;
public class ConfigurationEntityModel
{
    public static ConfigurationEntityModel Default => new ConfigurationEntityModel
    {
        ForceCloseReservations = true,
        ForceOpenReservations = false,
        GracePeriodSeconds = 4,
        MaxSeatsPerPerson = 4,
        MaxSeatsPerIPAddress = 10,
        MaxSecondsToConfirmSeat = 600,
        ScheduledOpenDateTime = DateTimeOffset.UtcNow,
        ScheduledOpenTimeZone = "Coordinated Universal Time",
    };

    /// <summary>
    /// Disable reservations regardless of schedule.
    /// </summary>
    public bool ForceCloseReservations { get; set; }

    /// <summary>
    /// Enable reservations regardless of schedule.
    /// </summary>
    public bool ForceOpenReservations { get; set; }

    /// <summary>
    /// Additional time for user actions to account for latency.
    /// </summary>
    public int GracePeriodSeconds { get; set; }

    /// <summary>
    /// Maximum seats per person.
    /// </summary>
    public int MaxSeatsPerPerson { get; set; }

    /// <summary>
    /// Maximum seats per IP Address.
    /// </summary>
    public int MaxSeatsPerIPAddress { get; set; }

    /// <summary>
    /// Maximum seconds for the user to confirm their seat.
    /// </summary>
    /// <remarks>
    /// A grace period should be added to this but not shown to the user.
    /// </remarks>
    public int MaxSecondsToConfirmSeat { get; set; }

    /// <summary>
    /// Instant in time in which reservations should open.
    /// </summary>
    public DateTimeOffset ScheduledOpenDateTime { get; set; }

    /// <summary>
    /// Timezone to be used to display <see cref="ScheduledOpenDateTime"/>.
    /// </summary>
    public string ScheduledOpenTimeZone { get; set; } = null!;
}

using Core.Domain.Common.Models.Entities;
using Core.Domain.Scheduling;

namespace Core.Application.System;
public class FetchConfigurationQueryResponse
{
    /// <summary>
    /// Minimal valid configuration, useful for unit tests.
    /// </summary>
    public static FetchConfigurationQueryResponse DefaultForTesting => new()
    {
        AreReservationsOpen = true,
        ReservationsStatus = ReservationsStatus.OpenedManually,
    };

    private FetchConfigurationQueryResponse()
    {
        ScheduledCloseTimeZone = "UTC";
        ScheduledOpenTimeZone = "UTC";
    }

    internal FetchConfigurationQueryResponse(ConfigurationEntityModel entityModel)
    {
        ForceCloseReservations = entityModel.ForceCloseReservations;
        ForceOpenReservations = entityModel.ForceOpenReservations;
        GracePeriodSeconds = entityModel.GracePeriodSeconds;
        MaxSeatsPerIPAddress = entityModel.MaxSeatsPerIPAddress;
        MaxSeatsPerReservation = entityModel.MaxSeatsPerReservation;
        MaxSeatsPerPerson = entityModel.MaxSeatsPerPerson;
        MaxSecondsToConfirmSeat = entityModel.MaxSecondsToConfirmSeat;
        ScheduledCloseDateTime = entityModel.ScheduledCloseDateTime;
        ScheduledCloseTimeZone = entityModel.ScheduledCloseTimeZone;
        ScheduledOpenDateTime = entityModel.ScheduledOpenDateTime;
        ScheduledOpenTimeZone = entityModel.ScheduledOpenTimeZone;
    }

    /// <summary>
    /// Whether the reservations are open or not.
    /// </summary>
    public required bool AreReservationsOpen { get; set; }

    /// <summary>
    /// Current status of the reservations system.
    /// </summary>
    public required ReservationsStatus ReservationsStatus { get; set; }

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
    /// <remarks>
    /// This should only be used on expiration checks.
    /// It should never be saved as part of an expiration.
    /// </remarks>
    public int GracePeriodSeconds { get; set; }

    /// <summary>
    /// Maximum seats per IP Address.
    /// </summary>
    public int MaxSeatsPerIPAddress { get; set; }

    /// <summary>
    /// Maximum seats per person.
    /// </summary>
    public int MaxSeatsPerPerson { get; set; }

    /// <summary>
    /// Maximum seats per reservation.
    /// </summary>
    public int MaxSeatsPerReservation { get; set; }

    /// <summary>
    /// Maximum seconds for the user to confirm their seat.
    /// </summary>
    /// <remarks>
    /// A grace period should be added to this but not shown to the user.
    /// </remarks>
    public int MaxSecondsToConfirmSeat { get; set; }

    /// <summary>
    /// Instant in time in which reservations should close.
    /// </summary>
    public DateTimeOffset ScheduledCloseDateTime { get; set; }

    /// <summary>
    /// Timezone to be used to display <see cref="ScheduledCloseDateTime"/>
    /// </summary>
    public string ScheduledCloseTimeZone { get; set; }

    /// <summary>
    /// Instant in time in which reservations should open.
    /// </summary>
    public DateTimeOffset ScheduledOpenDateTime { get; set; }

    /// <summary>
    /// Timezone to be used to display <see cref="ScheduledOpenDateTime"/>.
    /// </summary>
    public string ScheduledOpenTimeZone { get; set; }
}

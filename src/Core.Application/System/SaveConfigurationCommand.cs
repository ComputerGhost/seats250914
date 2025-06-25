using Core.Domain.Common.Models.Entities;
using MediatR;

namespace Core.Application.System;
public class SaveConfigurationCommand : IRequest
{
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

    internal ConfigurationEntityModel ToConfigurationEntityModel()
    {
        return new ConfigurationEntityModel
        {
            ForceCloseReservations = ForceCloseReservations,
            ForceOpenReservations = ForceOpenReservations,
            GracePeriodSeconds = GracePeriodSeconds,
            MaxSeatsPerPerson = MaxSeatsPerPerson,
            MaxSeatsPerIPAddress = MaxSeatsPerIPAddress,
            MaxSecondsToConfirmSeat = MaxSecondsToConfirmSeat,
            ScheduledOpenDateTime = ScheduledOpenDateTime,
            ScheduledOpenTimeZone = ScheduledOpenTimeZone,

            // TODO: Set these properly. I have a ticket to do it already. Somewhere.
            ScheduledCloseDateTime = ScheduledOpenDateTime.AddMonths(1),
            ScheduledCloseTimeZone = ScheduledOpenTimeZone,
        };
    }
}

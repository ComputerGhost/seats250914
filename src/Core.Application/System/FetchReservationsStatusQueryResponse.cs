using Core.Domain.Scheduling;

namespace Core.Application.System;
public class FetchReservationsStatusQueryResponse
{
    /// <summary>
    /// Current status of the reservation system.
    /// </summary>
    public required ReservationsStatus Status { get; set; }

    /// <summary>
    /// Instant in time in which reservations should close.
    /// </summary>
    public required DateTimeOffset ScheduledCloseDateTime { get; set; }

    /// <summary>
    /// Timezone to be used to display <see cref="ScheduledCloseDateTime"/>.
    /// </summary>
    public required string ScheduledCloseTimeZone { get; set; }

    /// <summary>
    /// Instant in time in which reservations should open.
    /// </summary>
    public required DateTimeOffset ScheduledOpenDateTime { get; set; }

    /// <summary>
    /// Timezone to be used to display <see cref="ScheduledOpenDateTime"/>.
    /// </summary>
    public required string ScheduledOpenTimeZone { get; set; }
}

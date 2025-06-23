namespace Core.Domain.Scheduling;
public enum ReservationsStatus
{
    /// <summary>
    /// Staff has force closed reservations.
    /// </summary>
    ClosedManually,

    /// <summary>
    /// Reservations are closed as scheduled.
    /// </summary>
    ClosedPerSchedule,

    /// <summary>
    /// No seats are available.
    /// </summary>
    OutOfSeatsPermanently,

    /// <summary>
    /// No seats are available, but some reservations are pending.
    /// </summary>
    OutOfSeatsTemporarily,

    /// <summary>
    /// Staff has forced open reservations.
    /// </summary>
    OpenedManually,

    /// <summary>
    /// Reservations are opened as scheduled.
    /// </summary>
    OpenedPerSchedule,

    /// <summary>
    /// Reservations are closed but will be open later.
    /// </summary>
    OpeningLater,
}

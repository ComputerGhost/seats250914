namespace Public.Models.Enumerations;

public enum SystemStatus
{
    /// <summary>
    /// The reservations will open soon according to the schedule.
    /// </summary>
    OpeningSoon,

    /// <summary>
    /// The reservations are open.
    /// </summary>
    Open,

    /// <summary>
    /// The reservations will close soon according to the schedule.
    /// </summary>
    ClosingSoon,

    /// <summary>
    /// The reservations are closed according to the schedule.
    /// </summary>
    ClosedPerSchedule,

    /// <summary>
    /// Someone force closed the reservations.
    /// </summary>
    ClosedManually,

    /// <summary>
    /// All seats are either on hold or reserved.
    /// </summary>
    OutOfSeatsTemporarily,

    /// <summary>
    /// All seats are reserved.
    /// </summary>
    OutOfSeatsPermanently,
}

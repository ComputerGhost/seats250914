namespace Core.Application.Reservations;
public class LockSeatCommandResponse
{
    /// <summary>
    /// The number of the seat that was locked.
    /// </summary>
    public int SeatNumber { get; set; }

    /// <summary>
    /// Use this to convert this hold into a reservation.
    /// </summary>
    public string SeatKey { get; set; } = null!;

    /// <summary>
    /// The lock is no longer valid after this expiration time.
    /// </summary>
    public DateTimeOffset LockExpiration { get; set; }
}

namespace Core.Application.Reservations;
public class LockSeatsCommandResponse
{
    /// <summary>
    /// Map of seat numbers to their keys.
    /// </summary>
    public IDictionary<int, string> SeatLocks { get; set; } = null!;

    /// <summary>
    /// The lock are are no longer valid after this expiration time.
    /// </summary>
    public DateTimeOffset LockExpiration { get; set; }
}

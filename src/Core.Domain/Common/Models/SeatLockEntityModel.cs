namespace Core.Domain.Common.Models;
public class SeatLockEntityModel
{
    public int SeatNumber { get; set; }

    public DateTimeOffset Expiration { get; set; }

    public string Key { get; set; } = null!;
}

namespace Core.Domain.Common.Models.Entities;
public class SeatLockEntityModel
{
    public DateTimeOffset Expiration { get; set; }

    public string IpAddress { get; set; } = null!;

    public string Key { get; set; } = null!;

    public DateTimeOffset LockedAt { get; set; }

    public int SeatNumber { get; set; }
}

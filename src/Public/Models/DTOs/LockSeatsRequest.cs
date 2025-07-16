namespace Public.Models.DTOs;

public class LockSeatsRequest
{
    public IEnumerable<int> SeatNumber { get; set; } = null!;
}

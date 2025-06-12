namespace Core.Application.Reservations;
public class ListReservationsQueryResponseItem
{
    /// <summary>
    /// When the reservation was made.
    /// </summary>
    public DateTimeOffset ReservedAt { get; set; }

    /// <summary>
    /// Number of the seat reserved.
    /// </summary>
    public int SeatNumber { get; set; }

    /// <summary>
    /// Name of the person reserving the seat.
    /// </summary>
    public string Name { get; set; } = null!;
}

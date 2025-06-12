namespace Core.Application.Reservations;
public class FetchReservationQueryResponse
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

    /// <summary>
    /// Email of the person reserving the seat.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Optional phone number of the person reserving the seat.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Preferred langauge of the person reserving the seat.
    /// </summary>
    public string PreferredLanguage { get; set; } = null!;
}

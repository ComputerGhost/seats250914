namespace Core.Domain.Common.Models;
public class ReservationEntityModel
{
    // If we're creating the reservation then we won't have an id yet.
    private const int UNDEFINED_ID = -1;

    /// <summary>
    /// The primary key of the reservation, if it exists.
    /// </summary>
    public int Id { get; set; } = UNDEFINED_ID;

    /// <summary>
    /// When the reservation was made.
    /// </summary>
    /// <remarks>
    /// This can only be set on reservation creation. It is ignored on update.
    /// </remarks>
    public DateTimeOffset ReservedAt { get; set; }

    /// <summary>
    /// Number of the seat reserved.
    /// </summary>
    /// <remarks>
    /// This can only be set on reservation creation. It is ignored on update.
    /// </remarks>
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

    /// <summary>
    /// Status of the reservation.
    /// </summary>
    /// <remarks>
    /// This can only be set on reservation creation and by direct updates. It is ignored on update.
    /// </remarks>
    public string Status { get; set; } = null!;
}

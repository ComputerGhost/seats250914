namespace Core.Domain.Common.Models.Entities;
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
    /// Numbers of the seats reserved.
    /// </summary>
    /// <remarks>
    /// This is only used when pulling data. It is ignored on create and update.
    /// </remarks>
    public IList<int> SeatNumbers { get; set; } = [];

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

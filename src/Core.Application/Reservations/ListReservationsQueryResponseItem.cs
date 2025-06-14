using Core.Application.Common.Enumerations;

namespace Core.Application.Reservations;
public class ListReservationsQueryResponseItem
{
    /// <summary>
    /// Name of the person reserving the seat.
    /// </summary>
    public required string Name { get; set; } = null!;

    /// <summary>
    /// The primary key of the reservation.
    /// </summary>
    public required int ReservationId { get; set; }

    /// <summary>
    /// When the reservation was made.
    /// </summary>
    public required DateTimeOffset ReservedAt { get; set; }

    /// <summary>
    /// Number of the seat reserved.
    /// </summary>
    public required int SeatNumber { get; set; }

    /// <summary>
    /// Status of the reservation.
    /// </summary>
    public required ReservationStatus Status { get; set; }
}

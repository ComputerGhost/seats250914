using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;

/// <summary>
/// Rejects a reservation and deletes its lock on the seat.
/// </summary>
public class RejectReservationCommand : IRequest<ErrorOr<Success>>
{
    /// <summary>
    /// The primary key of the reservation.
    /// </summary>
    public int ReservationId { get; set; }
}

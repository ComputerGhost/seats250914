using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;

/// <summary>
/// Approves a reservation and removes it from the 'pending' statuses.
/// </summary>
public class ApproveReservationCommand : IRequest<ErrorOr<Success>>
{
    /// <summary>
    /// The primary key of the reservation.
    /// </summary>
    public int ReservationId { get; set; }
}

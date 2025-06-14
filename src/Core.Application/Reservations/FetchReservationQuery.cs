using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
public class FetchReservationQuery : IRequest<ErrorOr<FetchReservationQueryResponse>>
{
    public FetchReservationQuery(int reservationId)
    {
        ReservationId = reservationId;
    }

    /// <summary>
    /// The primary key of the reservation.
    /// </summary>
    public int ReservationId { get; set; }
}

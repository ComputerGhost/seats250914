using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
public class FetchReservationQuery : IRequest<ErrorOr<FetchReservationQueryResponse>>
{
    /// <summary>
    /// Number of the seat reserved.
    /// </summary>
    public int SeatNumber { get; set; }
}

using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
public class FetchReservationQueryHandler : IRequestHandler<FetchReservationQuery, ErrorOr<FetchReservationQueryResponse>>
{
    private readonly IReservationsDatabase _reservationsDatabase;

    public FetchReservationQueryHandler(IReservationsDatabase reservationsDatabase)
    {
        _reservationsDatabase = reservationsDatabase;
    }

    public async Task<ErrorOr<FetchReservationQueryResponse>> Handle(FetchReservationQuery request, CancellationToken cancellationToken)
    {
        var reservationEntity = await _reservationsDatabase.FetchReservation(request.ReservationId);
        if (reservationEntity == null)
        {
            return Error.NotFound();
        }

        return new FetchReservationQueryResponse
        {
            ReservedAt = reservationEntity.ReservedAt,
            SeatNumber = reservationEntity.SeatNumber,
            Name = reservationEntity.Name,
            Email = reservationEntity.Email,
            PhoneNumber = reservationEntity.PhoneNumber,
            PreferredLanguage = reservationEntity.PreferredLanguage,
            Status = Enum.Parse<ReservationStatus>(reservationEntity.Status),
        };
    }
}

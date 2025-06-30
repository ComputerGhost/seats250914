using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
public class FetchReservationQueryHandler(IReservationsDatabase reservationsDatabase)
    : IRequestHandler<FetchReservationQuery, ErrorOr<FetchReservationQueryResponse>>
{
    public async Task<ErrorOr<FetchReservationQueryResponse>> Handle(FetchReservationQuery request, CancellationToken cancellationToken)
    {
        Log.Information("Fetching reservation {ReservationId} data.", request.ReservationId);

        var reservationEntity = await reservationsDatabase.FetchReservation(request.ReservationId);
        if (reservationEntity == null)
        {
            Log.Warning("Reservation {ReservationId} data could not be fetched because it does not exist.", request.ReservationId);
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

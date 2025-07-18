using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Ports;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class ListReservationsQueryHandler(IReservationsDatabase reservationsDatabase)
    : IRequestHandler<ListReservationsQuery, ListReservationsQueryResponse>
{
    public async Task<ListReservationsQueryResponse> Handle(ListReservationsQuery request, CancellationToken cancellationToken)
    {
        Log.Information("Listing all reservations.");
        var reservationEntities = await reservationsDatabase.ListReservations();

        return new ListReservationsQueryResponse
        {
            Data = reservationEntities.Select(reservationEntity => new ListReservationsQueryResponseItem
            {
                Name = reservationEntity.Name,
                ReservationId = reservationEntity.Id,
                ReservedAt = reservationEntity.ReservedAt,
                SeatNumbers = reservationEntity.SeatNumbers,
                Status = Enum.Parse<ReservationStatus>(reservationEntity.Status),
            })
        };
    }
}

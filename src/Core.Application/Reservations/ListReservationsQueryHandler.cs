using Core.Application.Common.Enumerations;
using Core.Domain.Common.Ports;
using MediatR;

namespace Core.Application.Reservations;
internal class ListReservationsQueryHandler : IRequestHandler<ListReservationsQuery, ListReservationsQueryResponse>
{
    private readonly IReservationsDatabase _reservationsDatabase;

    public ListReservationsQueryHandler(IReservationsDatabase reservationsDatabase)
    {
        _reservationsDatabase = reservationsDatabase;
    }

    public async Task<ListReservationsQueryResponse> Handle(ListReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservationEntities = await _reservationsDatabase.ListReservations();

        return new ListReservationsQueryResponse
        {
            Data = reservationEntities.Select(reservationEntity => new ListReservationsQueryResponseItem
            {
                Name = reservationEntity.Name,
                ReservationId = reservationEntity.Id,
                ReservedAt = reservationEntity.ReservedAt,
                SeatNumber = reservationEntity.SeatNumber,
                Status = Enum.Parse<ReservationStatus>(reservationEntity.Status),
            })
        };
    }
}

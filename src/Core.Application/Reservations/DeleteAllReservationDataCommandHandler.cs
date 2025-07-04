using Core.Domain.Common.Ports;
using Core.Domain.Reservations;
using MediatR;

namespace Core.Application.Reservations;
internal class DeleteAllReservationDataCommandHandler : IRequestHandler<DeleteAllReservationDataCommand>
{
    private readonly IReservationsDatabase _reservationsDatabase;
    private readonly ISeatsDatabase _seatsDatabase;
    private readonly ISeatLockService _seatLockService;

    public DeleteAllReservationDataCommandHandler(IReservationsDatabase reservationsDatabase, ISeatLockService seatLockService, ISeatsDatabase seatsDatabase)
    {
        _reservationsDatabase = reservationsDatabase;
        _seatLockService = seatLockService;
        _seatsDatabase = seatsDatabase;
    }

    public async Task Handle(DeleteAllReservationDataCommand request, CancellationToken cancellationToken)
    {
        await _reservationsDatabase.DeleteAllReservations();

        foreach (var seat in await _seatsDatabase.ListSeats())
        {
            await _seatLockService.UnlockSeat(seat.Number);
        }
    }
}

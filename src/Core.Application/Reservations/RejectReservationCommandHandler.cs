using Core.Application.Common.Enumerations;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using System.Diagnostics;
using System.Transactions;

namespace Core.Application.Reservations;
internal class RejectReservationCommandHandler : IRequestHandler<RejectReservationCommand, ErrorOr<Success>>
{
    private readonly IReservationsDatabase _reservationsDatabase;
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public RejectReservationCommandHandler(
        IReservationsDatabase reservationsDatabase,
        ISeatLocksDatabase seatLocksDatabase,
        ISeatsDatabase seatsDatabase)
    {
        _reservationsDatabase = reservationsDatabase;
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task<ErrorOr<Success>> Handle(RejectReservationCommand request, CancellationToken cancellationToken)
    {
        var reservationEntity = await _reservationsDatabase.FetchReservation(request.ReservationId);
        if (reservationEntity == null)
        {
            return Error.NotFound();
        }

        await UpdateReservationStatus(request.ReservationId);
        await UpdateSeatStatus(reservationEntity.SeatNumber);
        await _seatLocksDatabase.DeleteLock(reservationEntity.SeatNumber);

        return Result.Success;
    }

    private Task<bool> UpdateReservationStatus(int reservationId)
    {
        var reservationStatus = ReservationStatus.ReservationRejected.ToString();
        return _reservationsDatabase.UpdateReservationStatus(reservationId, reservationStatus);
    }

    private async Task UpdateSeatStatus(int seatNumber)
    {
        var seatStatus = SeatStatus.Available.ToString();
        var result = await _seatsDatabase.UpdateSeatStatus(seatNumber, seatStatus);
        Debug.Assert(result, $"Seat {seatNumber} does not exist yet had a reservation.");
    }
}

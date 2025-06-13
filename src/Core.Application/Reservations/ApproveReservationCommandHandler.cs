using Core.Application.Common.Enumerations;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using System.Diagnostics;

namespace Core.Application.Reservations;
internal class ApproveReservationCommandHandler : IRequestHandler<ApproveReservationCommand, ErrorOr<Success>>
{
    private readonly IReservationsDatabase _reservationsDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public ApproveReservationCommandHandler(
        IReservationsDatabase reservationsDatabase,
        ISeatsDatabase seatsDatabase)
    {
        _reservationsDatabase = reservationsDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task<ErrorOr<Success>> Handle(ApproveReservationCommand request, CancellationToken cancellationToken)
    {
        var reservationEntity = await _reservationsDatabase.FetchReservation(request.ReservationId);
        if (reservationEntity == null)
        {
            return Error.NotFound();
        }

        await UpdateReservationStatus(request.ReservationId);
        await UpdateSeatStatus(reservationEntity.SeatNumber);

        return Result.Success;
    }

    private Task<bool> UpdateReservationStatus(int reservationId)
    {
        var reservationStatus = ReservationStatus.ReservationConfirmed.ToString();
        return _reservationsDatabase.UpdateReservationStatus(reservationId, reservationStatus);
    }

    private async Task UpdateSeatStatus(int seatNumber)
    {
        var seatStatus = SeatStatus.ReservationConfirmed.ToString();
        var result = await _seatsDatabase.UpdateSeatStatus(seatNumber, seatStatus);
        Debug.Assert(result, $"Seat {seatNumber} does not exist yet had a reservation.");
    }
}

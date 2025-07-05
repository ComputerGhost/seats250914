using Core.Domain.Reservations;
using ErrorOr;
using MediatR;
using Serilog;
using System.Diagnostics;

namespace Core.Application.Reservations;
internal class AdminReserveSeatCommandHandler : IRequestHandler<AdminReserveSeatCommand, ErrorOr<int>>
{
    private readonly IReservationService _reservationService;
    private readonly ISeatLockService _seatLockService;

    public AdminReserveSeatCommandHandler(IReservationService reservationService, ISeatLockService seatLockService)
    {
        _reservationService = reservationService;
        _seatLockService = seatLockService;
    }

    public async Task<ErrorOr<int>> Handle(AdminReserveSeatCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Locking seat {SeatNumber} for admin identity {Identity}.", request.SeatNumber, request.Identity);

        // This is where the magic happens. Only one person can lock each seat.
        var lockEntity = await _seatLockService.LockSeat(request.SeatNumber, request.IpAddress);
        if (lockEntity == null)
        {
            return SeatTaken(request.SeatNumber);
        }

        Log.Information("Reserving seat {SeatNumber} for admin identity {Identity}.", request.SeatNumber, request.Identity);

        var reservationId = await _reservationService.ReserveSeat(request.SeatNumber, request.Identity);
        Debug.Assert(reservationId != null, "Reservation should not fail here.");

        await _reservationService.ApproveReservation(reservationId.Value);

        return reservationId.Value;
    }

    private static Error SeatTaken(int seatNumber)
    {
        Log.Information("Could not lock seat {seatNumber} because it is already locked.", seatNumber);
        return Error.Conflict();
    }
}

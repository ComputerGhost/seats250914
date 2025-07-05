using Core.Domain.Authorization;
using Core.Domain.Reservations;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class ReserveSeatCommandHandler : IRequestHandler<ReserveSeatCommand, ErrorOr<int>>
{
    private readonly IAuthorizationChecker _authorizationChecker;
    private readonly IReservationService _reservationService;

    public ReserveSeatCommandHandler(IAuthorizationChecker authorizationCheck, IReservationService reservationService)
    {
        _authorizationChecker = authorizationCheck;
        _reservationService = reservationService;
    }

    public async Task<ErrorOr<int>> Handle(ReserveSeatCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Reserving seat {SeatNumber} for identity {Identity}.", request.SeatNumber, request.Identity);

        var authResult = await _authorizationChecker.GetReserveSeatAuthorization(request.Identity, request.SeatNumber, request.SeatKey);
        if (!authResult.IsAuthorized)
        {
            return Unauthorized(request.SeatNumber, authResult);
        }

        var reservationId = await _reservationService.ReserveSeat(request.SeatNumber, request.Identity);
        if (reservationId == null)
        {
            return UnauthorizedJustNow(request.SeatNumber);
        }

        return reservationId.Value;
    }

    private static Error Unauthorized(int seatNumber, AuthorizationResult authResult)
    {
        var reason = authResult.FailureReason.ToString();
        Log.Information("User is not authorized to reserve seat {SeatNumber} because {reason}.", seatNumber, reason);
        return Error.Unauthorized(metadata: new Dictionary<string, object> { { "details", authResult } });
    }

    private static Error UnauthorizedJustNow(int seatNumber)
    {
        Log.Warning("User failed to reserve seat {SeatNumber}. Their seat key probably expired just now.", seatNumber);
        return Error.Unauthorized(metadata: new Dictionary<string, object> {
            { "details", AuthorizationResult.KeyIsExpired }
        });
    }
}

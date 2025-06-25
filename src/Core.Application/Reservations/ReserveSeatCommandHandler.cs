using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Reservations;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
public class ReserveSeatCommandHandler : IRequestHandler<ReserveSeatCommand, ErrorOr<int>>
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
        var unauthorizedMessage = $"User is not authorized to reserve seat {request.SeatNumber}.";
        
        if (!await CanReserveSeat(request.Identity, request.SeatNumber, request.SeatKey))
        {
            return Error.Unauthorized(unauthorizedMessage);
        }

        var reservationId = await _reservationService.ReserveSeat(request.SeatNumber, request.Identity);
        if (reservationId == null)
        {
            return Error.Failure(unauthorizedMessage);
        }

        return reservationId.Value;
    }

    private async Task<bool> CanReserveSeat(IdentityModel identity, int seatNumber, string key)
    {
        var result = await _authorizationChecker.GetReserveSeatAuthorization(identity, seatNumber, key);
        return result.IsAuthorized;
    }
}

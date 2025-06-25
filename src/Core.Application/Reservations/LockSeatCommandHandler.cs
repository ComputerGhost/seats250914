using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Reservations;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
internal class LockSeatCommandHandler : IRequestHandler<LockSeatCommand, ErrorOr<LockSeatCommandResponse>>
{
    private readonly IAuthorizationChecker _authorizationCheck;
    private readonly ISeatLockService _seatLockService;

    public LockSeatCommandHandler(IAuthorizationChecker authorizationCheck, ISeatLockService seatLockService)
    {
        _authorizationCheck = authorizationCheck;
        _seatLockService = seatLockService;
    }

    public async Task<ErrorOr<LockSeatCommandResponse>> Handle(LockSeatCommand request, CancellationToken cancellationToken)
    {
        if (!await CanLockSeat(request.Identity))
        {
            return Error.Unauthorized($"User is not authorized to lock seat {request.SeatNumber}.");
        }

        // This is where the magic happens. Only one person can lock each seat.
        var lockEntity = await _seatLockService.LockSeat(request.SeatNumber, request.IpAddress);
        if (lockEntity == null)
        {
            return Error.Conflict();
        }

        return new LockSeatCommandResponse
        {
            SeatNumber = request.SeatNumber,
            SeatKey = lockEntity.Key,
            LockExpiration = lockEntity.Expiration,
        };
    }

    public async Task<bool> CanLockSeat(IdentityModel identity)
    {
        var result = await _authorizationCheck.GetLockSeatAuthorization(identity);
        return result.IsAuthorized;
    }
}

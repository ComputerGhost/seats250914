using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Reservations;
using ErrorOr;
using MediatR;
using Serilog;

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
        Log.Information("Locking seat {SeatNumber} for {@Identity}.", request.IpAddress, request.Identity);

        if (!await CanLockSeat(request.Identity))
        {
            Log.Information("Could not lock seat {SeatNumber} because user is not authorized.", request.SeatNumber);
            return Error.Unauthorized($"User is not authorized to lock seat {request.SeatNumber}.");
        }

        // This is where the magic happens. Only one person can lock each seat.
        var lockEntity = await _seatLockService.LockSeat(request.SeatNumber, request.IpAddress);
        if (lockEntity == null)
        {
            Log.Information("Could not lock seat {SeatNumber} because it is already locked.", request.SeatNumber);
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

using Core.Domain.Authorization;
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
        Log.Information("Locking seat {SeatNumber} for {@Identity}.", request.SeatNumber, request.Identity);

        var authResult = await _authorizationCheck.GetLockSeatAuthorization(request.Identity);
        if (!authResult.IsAuthorized)
        {
            return Unauthorized(request.SeatNumber, authResult);
        }

        // This is where the magic happens. Only one person can lock each seat.
        var lockEntity = await _seatLockService.LockSeat(request.SeatNumber, request.IpAddress);
        if (lockEntity == null)
        {
            return SeatTaken(request.SeatNumber);
        }

        return new LockSeatCommandResponse
        {
            SeatNumber = request.SeatNumber,
            SeatKey = lockEntity.Key,
            LockExpiration = lockEntity.Expiration,
        };
    }

    private static Error SeatTaken(int seatNumber)
    {
        Log.Information("Could not lock seat {seatNumber} because it is already locked.", seatNumber);
        return Error.Conflict();
    }

    private static Error Unauthorized(int seatNumber, AuthorizationResult authResult)
    {
        var reason = authResult.FailureReason.ToString();
        Log.Information("User is not authorized to lock seat {SeatNumber} because {reason}.", seatNumber, reason);
        return Error.Unauthorized(metadata: new Dictionary<string, object> { { "details", authResult } });
    }
}

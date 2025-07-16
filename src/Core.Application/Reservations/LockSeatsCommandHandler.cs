using Core.Domain.Authorization;
using Core.Domain.Reservations;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class LockSeatsCommandHandler : IRequestHandler<LockSeatsCommand, ErrorOr<LockSeatsCommandResponse>>
{
    private readonly IAuthorizationChecker _authorizationCheck;
    private readonly ISeatLockService _seatLockService;

    public LockSeatsCommandHandler(IAuthorizationChecker authorizationCheck, ISeatLockService seatLockService)
    {
        _authorizationCheck = authorizationCheck;
        _seatLockService = seatLockService;
    }

    public async Task<ErrorOr<LockSeatsCommandResponse>> Handle(LockSeatsCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Locking seats {SeatNumbers} for {@Identity}.", request.SeatNumbers, request.Identity);

        var authResult = await _authorizationCheck.GetLockSeatAuthorization(request.Identity);
        if (!authResult.IsAuthorized)
        {
            return Unauthorized(authResult);
        }

        var response = new LockSeatsCommandResponse
        {
            LockExpiration = DateTimeOffset.MaxValue,
            SeatLocks = new Dictionary<int, string>(),
        };

        foreach (var seatNumber in request.SeatNumbers)
        {
            var lockEntity = await _seatLockService.LockSeat(seatNumber, request.IpAddress);
            if (lockEntity == null)
            {
                await UndoLocks(response.SeatLocks);
                return SeatTaken(seatNumber);
            }

            response.SeatLocks[seatNumber] = lockEntity.Key;
            response.LockExpiration = lockEntity.Expiration;
        }

        return response;
    }

    private static Error SeatTaken(int seatNumber)
    {
        Log.Information("Could not lock seat {seatNumber} because it is already locked.", seatNumber);
        return Error.Conflict(metadata: new Dictionary<string, object> { { "seatNumber", seatNumber } });
    }

    private static Error Unauthorized(AuthorizationResult authResult)
    {
        var reason = authResult.FailureReason.ToString();
        Log.Information("User is not authorized to lock seats because {reason}.", reason);
        return Error.Unauthorized(metadata: new Dictionary<string, object> { { "details", authResult } });
    }

    private async Task UndoLocks(IDictionary<int, string> seatKeys)
    {
        foreach (var seatNumber in seatKeys.Keys)
        {
            await _seatLockService.UnlockSeat(seatNumber);
        }
    }
}

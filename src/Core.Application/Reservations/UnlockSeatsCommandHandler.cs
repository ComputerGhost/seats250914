using Core.Domain.Authorization;
using Core.Domain.Reservations;
using ErrorOr;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class UnlockSeatsCommandHandler : IRequestHandler<UnlockSeatsCommand, ErrorOr<Success>>
{
    private readonly IAuthorizationChecker _authorizationChecker;
    private readonly ISeatLockService _seatLockService;

    public UnlockSeatsCommandHandler(IAuthorizationChecker authorizationChecker, ISeatLockService seatLockService)
    {
        _authorizationChecker = authorizationChecker;
        _seatLockService = seatLockService;
    }

    public async Task<ErrorOr<Success>> Handle(UnlockSeatsCommand request, CancellationToken cancellationToken)
    {
        var seatNumbers = request.SeatLocks.Select(x => x.Key);
        Log.Information("Unlocking seats {seatNumbers}.", seatNumbers);

        foreach (var seatLock in request.SeatLocks)
        {
            if (!await CanUnlockSeat(seatLock.Key, seatLock.Value))
            {
                var seatNumber = seatLock.Key;
                Log.Warning("User is not authorized to unlock seat {seatNumber}.", seatNumber);
                return Error.Unauthorized($"User is not authorized to reserve seat {seatNumber}.");
            }
        }

        foreach (var seatLock in request.SeatLocks)
        {
            await _seatLockService.UnlockSeat(seatLock.Key);
        }

        return Result.Success;
    }

    private async Task<bool> CanUnlockSeat(int seatNumber, string key)
    {
        var result = await _authorizationChecker.GetUnlockSeatAuthorization(seatNumber, key);
        return result.IsAuthorized;
    }
}

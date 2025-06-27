using Core.Domain.Authorization;
using Core.Domain.Reservations;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
internal class UnlockSeatCommandHandler : IRequestHandler<UnlockSeatCommand, ErrorOr<Success>>
{
    private readonly IAuthorizationChecker _authorizationChecker;
    private readonly ISeatLockService _seatLockService;

    public UnlockSeatCommandHandler(IAuthorizationChecker authorizationChecker, ISeatLockService seatLockService)
    {
        _authorizationChecker = authorizationChecker;
        _seatLockService = seatLockService;
    }

    public async Task<ErrorOr<Success>> Handle(UnlockSeatCommand request, CancellationToken cancellationToken)
    {
        if (!await CanUnlockSeat(request.SeatNumber, request.SeatKey))
        {
            return Error.Unauthorized($"User is not authorized to reserve seat {request.SeatNumber}.");
        }

        await _seatLockService.UnlockSeat(request.SeatNumber);

        return Result.Success;
    }

    private async Task<bool> CanUnlockSeat(int seatNumber, string key)
    {
        var result = await _authorizationChecker.GetUnlockSeatAuthorization(seatNumber, key);
        return result.IsAuthorized;
    }
}

using Core.Application.Common.Enumerations;
using Core.Domain.Authentication;
using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using ErrorOr;
using MediatR;
using System.Diagnostics;

namespace Core.Application.Reservations;
internal class LockSeatCommandHandler : IRequestHandler<LockSeatCommand, ErrorOr<LockSeatCommandResponse>>
{
    private readonly IConfigurationDatabase _configurationDatabase;
    private readonly IAuthorizationChecker _authorizationCheck;
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public LockSeatCommandHandler(
        IConfigurationDatabase configurationDatabase,
        IAuthorizationChecker authorizationCheck,
        ISeatLocksDatabase seatLocksDatabase,
        ISeatsDatabase seatsDatabase)
    {
        _configurationDatabase = configurationDatabase;
        _authorizationCheck = authorizationCheck;
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task<ErrorOr<LockSeatCommandResponse>> Handle(LockSeatCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _configurationDatabase.FetchConfiguration();

        if (!await CanLockSeat(configuration, request))
        {
            return Error.Failure($"User is not authorized to lock seat {request.SeatNumber}.");
        }

        if (!await DoesSeatExist(request.SeatNumber))
        {
            return Error.NotFound();
        }

        // This is where the magic happens. Only one person can lock each seat.
        var lockEntity = await LockSeat(request, configuration);
        if (lockEntity == null)
        {
            return Error.Conflict();
        }

        await UpdateSeatStatus(request.SeatNumber);

        return new LockSeatCommandResponse
        {
            SeatNumber = request.SeatNumber,
            SeatKey = lockEntity.Key,
            LockExpiration = lockEntity.Expiration,
        };
    }

    public async Task<bool> CanLockSeat(ConfigurationEntityModel configuration, LockSeatCommand request)
    {
        _authorizationCheck.SetUserIdentity(request.IsStaff, null, request.IpAddress);
        var result = await _authorizationCheck.GetLockSeatAuthorization(configuration);
        return result.IsAuthorized;
    }

    private async Task<bool> DoesSeatExist(int seatNumber)
    {
        var seatEntity = await _seatsDatabase.FetchSeat(seatNumber);
        return seatEntity != null;
    }

    private async Task<SeatLockEntityModel?> LockSeat(LockSeatCommand request, ConfigurationEntityModel configuration)
    {
        var expiration = DateTimeOffset.UtcNow.AddSeconds(configuration.MaxSecondsToConfirmSeat);
        var seatKey = SeatKeyUtilities.GenerateKey();

        var lockEntity = new SeatLockEntityModel
        {
            Expiration = expiration,
            IpAddress = request.IpAddress,
            Key = seatKey,
            LockedAt = DateTime.UtcNow,
            SeatNumber = request.SeatNumber,
        };

        if (!await _seatLocksDatabase.LockSeat(lockEntity))
        {
            return null;
        }

        return lockEntity;
    }

    private async Task UpdateSeatStatus(int seatNumber)
    {
        var status = SeatStatus.Locked.ToString();
        var result = await _seatsDatabase.UpdateSeatStatus(seatNumber, status);
        Debug.Assert(result, "Updating the seat status should not have failed here.");
    }
}

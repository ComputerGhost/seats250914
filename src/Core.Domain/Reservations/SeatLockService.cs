using Core.Domain.Authentication;
using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using System.Diagnostics;

namespace Core.Domain.Reservations;

[ServiceImplementation]
internal class SeatLockService : ISeatLockService
{
    private readonly IConfigurationDatabase _configurationDatabase;
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public SeatLockService(
        IConfigurationDatabase configurationDatabase, 
        ISeatLocksDatabase seatLocksDatabase,
        ISeatsDatabase seatsDatabase)
    {
        _configurationDatabase = configurationDatabase;
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task ClearExpiredLocks()
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        var gracePeriod = TimeSpan.FromSeconds(configuration.GracePeriodSeconds);
        await _seatLocksDatabase.ClearExpiredLocks(DateTimeOffset.UtcNow + gracePeriod);
        await _seatsDatabase.ResetUnlockedSeatStatuses();
    }

    public async Task<SeatLockEntityModel?> LockSeat(int seatNumber, string ipAddress)
    {
        var lockEntity = await CreateLock(seatNumber, ipAddress);
        if (lockEntity == null)
        {
            return null;
        }

        await UpdateSeatStatus(seatNumber, SeatStatus.Locked);

        return lockEntity;
    }

    private async Task<SeatLockEntityModel?> CreateLock(int seatNumber, string ipAddress)
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        var expiration = DateTimeOffset.UtcNow.AddSeconds(configuration.MaxSecondsToConfirmSeat);
        var seatKey = SeatKeyUtilities.GenerateKey();

        var lockEntity = new SeatLockEntityModel
        {
            Expiration = expiration,
            IpAddress = ipAddress,
            Key = seatKey,
            LockedAt = DateTime.UtcNow,
            SeatNumber = seatNumber,
        };

        return (await _seatLocksDatabase.LockSeat(lockEntity)) ? lockEntity : null;
    }

    private async Task UpdateSeatStatus(int seatNumber, SeatStatus status)
    {
        var result = await _seatsDatabase.UpdateSeatStatus(seatNumber, status.ToString());
        Debug.Assert(result, "Updating the seat status should not have failed here.");
    }
}

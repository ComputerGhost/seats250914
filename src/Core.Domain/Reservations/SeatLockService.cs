using Core.Domain.Authentication;
using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using MediatR;
using System.Diagnostics;

namespace Core.Domain.Reservations;

[ServiceImplementation]
internal class SeatLockService : ISeatLockService
{
    private readonly IMediator _mediator;
    private readonly IConfigurationDatabase _configurationDatabase;
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public SeatLockService(
        IMediator mediator,
        IConfigurationDatabase configurationDatabase, 
        ISeatLocksDatabase seatLocksDatabase,
        ISeatsDatabase seatsDatabase)
    {
        _mediator = mediator;
        _configurationDatabase = configurationDatabase;
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task ClearExpiredLocks()
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        var gracePeriod = TimeSpan.FromSeconds(configuration.GracePeriodSeconds);

        var expiredLocks = await _seatLocksDatabase.FetchExpiredLocks(DateTimeOffset.UtcNow + gracePeriod);
        var expiredSeatNumbers = expiredLocks.Select(x => x.SeatNumber);
        await _seatLocksDatabase.DeleteLocks(expiredSeatNumbers);
        await UpdateSeatStatuses(expiredSeatNumbers, SeatStatus.Available);
    }

    public async Task<SeatLockEntityModel?> LockSeat(int seatNumber, string ipAddress)
    {
        var lockEntity = await CreateLock(seatNumber, ipAddress);
        if (lockEntity == null)
        {
            return null;
        }

        await UpdateSeatStatuses([seatNumber], SeatStatus.Locked);

        return lockEntity;
    }

    public async Task UnlockSeats(IEnumerable<int> seatNumbers)
    {
        await _seatLocksDatabase.DeleteLocks(seatNumbers);
        await UpdateSeatStatuses(seatNumbers, SeatStatus.Available);
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

    private async Task UpdateSeatStatuses(IEnumerable<int> seatNumbers, SeatStatus status)
    {
        var result = await _seatsDatabase.UpdateSeatStatuses(seatNumbers, status.ToString());
        Debug.Assert(result == seatNumbers.Count(), $"Updating the statuses of seats {string.Join(", ", seatNumbers)} should not have failed here.");

        foreach (var seatNumber in seatNumbers)
        {
            await _mediator.Publish(new SeatStatusChangedNotification(seatNumber, status));
        }
    }
}

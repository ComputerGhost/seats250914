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
    /*
     * These operations are not wrapped in a transaction on purpose.
     * 
     * Only creating a lock is likely to fail, and that bails early before 
     * further changes. The other calls are not expected to fail unless the 
     * database itself fails.
     * 
     * If the database fails, the order of the database calls ensures that the
     * seats are left in an unavailable status if the data is corrupted. They 
     * are removed from the UI options until they can be fixed.
     * 
     * Thus the risk is low. It is not worth the overhead of a transaction.
     */

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

        await _mediator.Publish(new SeatStatusesChangedNotification());
    }

    public async Task<SeatLockEntityModel?> LockSeat(int seatNumber, string ipAddress)
    {
        var lockEntity = await CreateLock(seatNumber, ipAddress);
        if (lockEntity == null)
        {
            return null;
        }

        await UpdateSeatStatuses([seatNumber], SeatStatus.Locked);
        
        await _mediator.Publish(new SeatStatusesChangedNotification());

        return lockEntity;
    }

    public async Task UnlockSeats(IEnumerable<int> seatNumbers)
    {
        await _seatLocksDatabase.DeleteLocks(seatNumbers);
        await UpdateSeatStatuses(seatNumbers, SeatStatus.Available);

        await _mediator.Publish(new SeatStatusesChangedNotification());
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
    }
}

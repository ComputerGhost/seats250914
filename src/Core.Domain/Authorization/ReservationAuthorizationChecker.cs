using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;

namespace Core.Domain.Authorization;

[ServiceImplementation]
internal class ReservationAuthorizationChecker: IReservationAuthorizationChecker
{
    private readonly IConfigurationDatabase _configurationDatabase;
    private readonly ISeatLocksDatabase _seatLocksDatabase;

    public ReservationAuthorizationChecker(
        IConfigurationDatabase configurationDatabase,
        ISeatLocksDatabase seatLocksDatabase)
    {
        _configurationDatabase = configurationDatabase;
        _seatLocksDatabase = seatLocksDatabase;
    }

    public async Task<bool> CanMakeReservation()
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        return CanMakeReservation(configuration);
    }

    public bool CanMakeReservation(ConfigurationEntityModel configuration)
    {
        if (configuration.ForceCloseReservations || configuration.ForceOpenReservations)
        {
            return configuration.ForceOpenReservations;
        }

        var now = DateTime.UtcNow;
        return configuration.ScheduledOpenDateTime < now;
    }

    public async Task<bool> CanReserveSeat(int seatNumber, string key)
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        if (!CanMakeReservation(configuration))
        {
            return false;
        }

        var lockEntity = await _seatLocksDatabase.FetchSeatLock(seatNumber);
        if (lockEntity == null)
        {
            return false;
        }

        if (lockEntity.Key == key)
        {
            return false;
        }

        var now = DateTime.UtcNow;

        if (lockEntity.Expiration.AddSeconds(configuration.GracePeriodSeconds) >= now)
        {
            return false;
        }

        return true;
    }
}

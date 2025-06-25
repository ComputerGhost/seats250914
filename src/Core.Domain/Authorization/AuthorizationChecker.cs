using Core.Domain.Authentication;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Core.Domain.Scheduling;

namespace Core.Domain.Authorization;

[ServiceImplementation]
internal class AuthorizationChecker: IAuthorizationChecker
{
    private readonly IConfigurationDatabase _configurationDatabase;
    private readonly IReservationsDatabase _reservationsDatabase;
    private readonly ISeatLocksDatabase _seatLocksDatabase;

    public AuthorizationChecker(
        IConfigurationDatabase configurationDatabase,
        IReservationsDatabase reservationsDatabase,
        ISeatLocksDatabase seatLocksDatabase)
    {
        _configurationDatabase = configurationDatabase;
        _reservationsDatabase = reservationsDatabase;
        _seatLocksDatabase = seatLocksDatabase;
    }

    public async Task<AuthorizationResult> GetLockSeatAuthorization(IdentityModel identity)
    {
        var configuration = await _configurationDatabase.FetchConfiguration();

        var openChecker = OpenChecker.FromConfiguration(configuration);
        if (!openChecker.AreReservationsOpen())
        {
            return AuthorizationResult.ReservationsAreClosed;
        }

        if (!identity.IsStaff)
        {
            if (await _seatLocksDatabase.CountLocksForIpAddress(identity.IpAddress) >= configuration.MaxSeatsPerIPAddress)
            {
                return AuthorizationResult.TooManySeatLocksForIpAddress;
            }
        }

        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> GetReserveSeatAuthorization(IdentityModel identity, int seatNumber, string key)
    {
        var configuration = await _configurationDatabase.FetchConfiguration();

        var openChecker = OpenChecker.FromConfiguration(configuration);
        if (!openChecker.AreReservationsOpen())
        {
            return AuthorizationResult.ReservationsAreClosed;
        }

        if (!identity.IsStaff)
        {
            if (identity.Email == null)
            {
                throw new Exception("Email address is required in authorization check.");
            }

            if (await _reservationsDatabase.CountActiveReservationsForEmailAddress(identity.Email) >= configuration.MaxSeatsPerPerson)
            {
                return AuthorizationResult.TooManyReservationsForEmail;
            }
        }

        var lockEntity = await _seatLocksDatabase.FetchSeatLock(seatNumber);
        if (lockEntity == null)
        {
            return AuthorizationResult.SeatIsNotLocked;
        }

        if (!SeatKeyUtilities.VerifyKey(lockEntity.Key, key))
        {
            return AuthorizationResult.KeyIsInvalid;
        }

        if (lockEntity.Expiration.AddSeconds(configuration.GracePeriodSeconds) <= DateTime.UtcNow)
        {
            return AuthorizationResult.KeyIsExpired;
        }

        return AuthorizationResult.Success;
    }
}

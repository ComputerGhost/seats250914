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

    private bool IsStaff { get; set; } = false;
    private string IpAddress { get; set; } = null!;
    private string EmailAddress { get; set; } = null!;

    public async Task<AuthorizationResult> GetLockSeatAuthorization()
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        return await GetLockSeatAuthorization(configuration);
    }

    public async Task<AuthorizationResult> GetLockSeatAuthorization(ConfigurationEntityModel configuration)
    {
        var openChecker = OpenChecker.FromConfiguration(configuration);
        if (!openChecker.AreReservationsOpen())
        {
            return AuthorizationResult.ReservationsAreClosed;
        }

        if (!IsStaff)
        {
            if (await _seatLocksDatabase.CountLocksForIpAddress(IpAddress) >= configuration.MaxSeatsPerIPAddress)
            {
                return AuthorizationResult.TooManySeatLocksForIpAddress;
            }
        }

        return AuthorizationResult.Success;
    }

    public async Task<AuthorizationResult> GetReserveSeatAuthorization(int seatNumber, string key)
    {
        var configuration = await _configurationDatabase.FetchConfiguration();

        var openChecker = OpenChecker.FromConfiguration(configuration);
        if (!openChecker.AreReservationsOpen())
        {
            return AuthorizationResult.ReservationsAreClosed;
        }

        if (!IsStaff)
        {
            if (EmailAddress == IAuthorizationChecker.UNKNOWN_EMAIL)
            {
                throw new Exception("Email address is required in authorization check.");
            }

            var count = await _reservationsDatabase.CountActiveReservationsForEmailAddress(EmailAddress);
            if (await _reservationsDatabase.CountActiveReservationsForEmailAddress(EmailAddress) >= configuration.MaxSeatsPerPerson)
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

    public void SetUserIdentity(bool isStaff, string emailAddress, string ipAddress)
    {
        IsStaff = isStaff;
        EmailAddress = emailAddress;
        IpAddress = ipAddress;
    }
}

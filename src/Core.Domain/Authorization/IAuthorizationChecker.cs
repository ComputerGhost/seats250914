using Core.Domain.Common.Models;

namespace Core.Domain.Authorization;
public interface IAuthorizationChecker
{
    const string UNKNOWN_EMAIL = "unknown email";

    /// <summary>
    /// Whether the user can generally lock a seat.
    /// </summary>
    /// <remarks>
    /// If the user is not staff, the IP address of the identity information is required.
    /// </remarks>
    /// <param name="normalizedIpAddress">Ip address (normalized) of the user.</param>
    /// <returns></returns>
    Task<AuthorizationResult> GetLockSeatAuthorization();

    /// <summary>
    /// Whether the user can generally lock a seat.
    /// </summary>
    /// <remarks>
    /// If the user is not staff, the IP address of the identity information is required.
    /// </remarks>
    /// <param name="configuration">Configuration to use instead of loading from the database.</param>
    /// <returns></returns>
    Task<AuthorizationResult> GetLockSeatAuthorization(ConfigurationEntityModel configuration);

    /// <summary>
    /// Whether the user can reserve a specific seat.
    /// </summary>
    /// <remarks>
    /// If the user is not staff, all of the identity information is required.
    /// A grace period is considered for the lock expiration.
    /// </remarks>
    /// <param name="seatNumber">Number of the seat to check.</param>
    /// <param name="key">Key to unlock a hold on the seat.</param>
    Task<AuthorizationResult> GetReserveSeatAuthorization(int seatNumber,  string key);

    /// <summary>
    /// Sets the identity assumed for the user.
    /// </summary>
    /// <remarks>
    /// If the user is not staff, other parameters may be required. See other methods for remarks.
    /// </remarks>
    /// <param name="isStaff">Whether the change is being made by staff.</param>
    /// <param name="emailAddress">Email address of the user reserving the seat.</param>
    /// <param name="ipAddress">Ip address of the user reserving the seat.</param>
    void SetUserIdentity(bool isStaff, string emailAddress, string ipAddress);
}

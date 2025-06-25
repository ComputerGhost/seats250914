using Core.Domain.Common.Models;

namespace Core.Domain.Authorization;
public interface IAuthorizationChecker
{
    /// <summary>
    /// Whether the user can generally lock a seat.
    /// </summary>
    /// <remarks>
    /// If the user is not staff, the IP address of the identity information is required.
    /// </remarks>
    Task<AuthorizationResult> GetLockSeatAuthorization(IdentityModel identity);

    /// <summary>
    /// Whether the user can reserve a specific seat.
    /// </summary>
    /// <remarks>
    /// If the user is not staff, all of the identity information is required.
    /// A grace period is considered for the lock expiration.
    /// </remarks>
    /// <param name="identity">Identity of the one reserving the seat.</param>
    /// <param name="seatNumber">Number of the seat to check.</param>
    /// <param name="key">Key to unlock a hold on the seat.</param>
    Task<AuthorizationResult> GetReserveSeatAuthorization(IdentityModel identity, int seatNumber,  string key);
}

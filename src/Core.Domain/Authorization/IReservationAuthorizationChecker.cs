using Core.Domain.Common.Models;

namespace Core.Domain.Authorization;
public interface IReservationAuthorizationChecker
{
    /// <summary>
    /// Whether the user can generally make a reservation.
    /// </summary>
    Task<bool> CanMakeReservation();

    /// <summary>
    /// Whether the user can generally make a reservation.
    /// </summary>
    /// <param name="configuration">Configuration to use instead of loading from the database.</param>
    bool CanMakeReservation(ConfigurationEntityModel configuration);

    /// <summary>
    /// Whether the user can reserve a specific seat.
    /// </summary>
    /// <param name="seatNumber">Number of the seat to check.</param>
    /// <param name="key">Key to unlock a hold on the seat.</param>
    Task<bool> CanReserveSeat(int seatNumber, string key);
}

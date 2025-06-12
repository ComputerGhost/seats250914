using Core.Domain.Common.Models;

namespace Core.Domain.Common.Ports;
public interface ISeatLocksDatabase
{
    /**
     * PROGRAMMER'S NOTE:
     * 
     * To combat clock drift possibilities, we need a single source of truth 
     * for the current time. I have decided that will be the web server. 
     * Therefore, all functions herein must not use `GETUTCDATE` or similar; 
     * instead, they must be passed the date as a parameter if needed.
     */

    /// <summary>
    /// Deletes locks from the database that have expired.
    /// </summary>
    /// <remarks>
    /// Recommended to pass the current date plus the expiration timespan.
    /// </remarks>
    /// <param name="beforeDate">Locks expiring before this date will be deleted.</param>
    Task ClearExpiredLocks(DateTimeOffset beforeDate);

    /// <summary>
    /// Marks a lock as not expiring.
    /// </summary>
    /// <remarks>
    /// A nonexpiring lock is associated with reservation.
    /// </remarks>
    /// <param name="seatNumber">The seat number of the lock to alter.</param>
    Task ClearLockExpiration(int seatNumber);

    /// <summary>
    /// Applies a lock to a seat. Only one lock per seat is possible, which 
    /// makes it impossible for multiple people to lock the same seat.
    /// </summary>
    /// <param name="seatNumber">Seat number to lock.</param>
    /// <param name="expiration">Expiration of the lock before the seat is available to others.</param>
    /// <param name="key">Key that must be used to reserve the seat.</param>
    /// <returns>True if successful; false if the seat doesn't exist or is already locked.</returns>
    Task<bool> LockSeat(int seatNumber, DateTimeOffset expiration, string key);

    /// <summary>
    /// Returns the lock assigned to the locked seat.
    /// </summary>
    /// <returns>The seat lock; null if the lock was not found.</returns>
    Task<SeatLockEntityModel?> FetchSeatLock(int seatNumber);
}

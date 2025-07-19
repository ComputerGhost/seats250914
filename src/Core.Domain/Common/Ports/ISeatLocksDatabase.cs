using Core.Domain.Common.Models.Entities;

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
    /// Attaches a reservation to seat locks.
    /// </summary>
    /// <returns>The number of locks found and attached to.</returns>
    Task<int> AttachLocksToReservation(IEnumerable<int> seatNumbers, int reservationId);

    /// <summary>
    /// Marks locks as not expiring.
    /// </summary>
    /// <remarks>
    /// A nonexpiring lock is associated with reservation.
    /// </remarks>
    /// <param name="seatNumbers">The seat numbers of the locks to alter.</param>
    /// <returns>The number of locks made unexpiring.</returns>
    Task<int> ClearLockExpirations(IEnumerable<int> seatNumber);

    /// <summary>
    /// Count the number of locks active for the IP address.
    /// </summary>
    public Task<int> CountLocksForIpAddress(string ipAddress);

    /// <summary>
    /// Deletes a seat lock.
    /// If there is an associated reservation: the reservation remains, but its references to the locks are removed.
    /// </summary>
    /// <param name="seatNumbers">The seat numbers of the locks to delete.</param>
    /// <returns>True if successful; false if the locks were not found.</returns>
    Task<int> DeleteLocks(IEnumerable<int> seatNumbers);

    /// <summary>
    /// Fetches locks from the database that have expired.
    /// </summary>
    /// <remarks>
    /// Recommended to pass the current date plus the expiration timespan.
    /// </remarks>
    Task<IEnumerable<SeatLockEntityModel>> FetchExpiredLocks(DateTimeOffset beforeTime);

    /// <summary>
    /// Returns the lock assigned to the locked seat.
    /// </summary>
    /// <returns>The seat lock; null if the lock was not found.</returns>
    Task<SeatLockEntityModel?> FetchSeatLock(int seatNumber);

    /// <summary>
    /// Applies a lock to a seat. Only one lock per seat is possible, which 
    /// makes it impossible for multiple people to lock the same seat.
    /// </summary>
    /// <param name="seatLockEntity">Information for the lock to create.</param>
    /// <returns>True if successful; false if the seat doesn't exist or is already locked.</returns>
    Task<bool> LockSeat(SeatLockEntityModel seatLockEntity);
}

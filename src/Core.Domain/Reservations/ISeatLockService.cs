using Core.Domain.Common.Models.Entities;

namespace Core.Domain.Reservations;
public interface ISeatLockService
{
    Task ClearExpiredLocks();
    Task<SeatLockEntityModel?> LockSeat(int seatNumber, string ipAddress);
    Task UnlockSeats(IEnumerable<int> seatNumber);
}

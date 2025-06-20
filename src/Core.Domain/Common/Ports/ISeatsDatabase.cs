using Core.Domain.Common.Models;

namespace Core.Domain.Common.Ports;
public interface ISeatsDatabase
{
    /// <summary>
    /// Fetches a seat's basic info.
    /// </summary>
    /// <returns>The found seat, or null if not found.</returns>
    Task<SeatEntityModel?> FetchSeat(int seatNumber);

    /// <summary>
    /// List all seats.
    /// </summary>
    Task<IEnumerable<SeatEntityModel>> ListSeats();

    /// <summary>
    /// For seats that no longer have a lock, reset their statuses to 'Available'.
    /// </summary>
    Task ResetLockedSeatStatuses();

    /// <summary>
    /// Updates the status of the seat.
    /// </summary>
    /// <returns>
    /// True if successful; false if the seat was not found.
    /// </returns>
    Task<bool> UpdateSeatStatus(int seatNumber, string seatStatus);
}

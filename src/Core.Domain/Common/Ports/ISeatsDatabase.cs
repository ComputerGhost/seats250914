using Core.Domain.Common.Models.Entities;

namespace Core.Domain.Common.Ports;
public interface ISeatsDatabase
{
    /// <summary>
    /// Counts the number of seats with a specified status.
    /// </summary>
    Task<int> CountSeats(string seatStatus);

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
    Task ResetUnlockedSeatStatuses();

    /// <summary>
    /// Updates the status of the seat.
    /// </summary>
    /// <returns>
    /// True if successful; false if the seat was not found.
    /// </returns>
    Task<bool> UpdateSeatStatus(int seatNumber, string seatStatus);
}

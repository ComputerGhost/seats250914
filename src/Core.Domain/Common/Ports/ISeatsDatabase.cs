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
    /// List all seats with the specified status.
    /// </summary>
    Task<IEnumerable<SeatEntityModel>> ListSeats(string status);

    /// <summary>
    /// Updates the statuses of the seats.
    /// </summary>
    /// <returns>
    /// The number of seats updated.
    /// </returns>
    Task<int> UpdateSeatStatuses(IEnumerable<int> seatNumber, string seatStatus);
}

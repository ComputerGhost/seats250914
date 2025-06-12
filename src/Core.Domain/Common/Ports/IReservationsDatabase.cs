using Core.Domain.Common.Models;

namespace Core.Domain.Common.Ports;
public interface IReservationsDatabase
{
    /// <summary>
    /// Creates a reservation. The seat must have already been locked.
    /// </summary>
    /// <remarks>
    /// WARNING: This does not verify ownership of the seat lock.
    /// </remarks>
    /// <param name="reservation">Reservation to save.</param>
    Task CreateReservation(ReservationEntityModel reservation);

    /// <summary>
    /// List all reservations.
    /// </summary>
    Task<IEnumerable<ReservationEntityModel>> ListReservations();

    /// <summary>
    /// Fetch reservation data for a seat.
    /// </summary>
    /// <param name="seatNumber">Seat number to fetch.</param>
    /// <returns>Reservation data for the seat, null if reservation not found.</returns>
    Task<ReservationEntityModel?> FetchReservation(int seatNumber);

    /// <summary>
    /// Updates reservation data.
    /// </summary>
    /// <param name="seatNumber">Seat number of the reservation.</param>
    /// <param name="reservation">Reservation data to save.</param>
    /// <returns>True if the reservation was updated; false otherwise.</returns>
    Task<bool> UpdateReservation(int seatNumber, ReservationEntityModel reservation);
}

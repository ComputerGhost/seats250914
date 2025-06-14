using Core.Domain.Common.Models;

namespace Core.Domain.Common.Ports;
public interface IReservationsDatabase
{
    /// <summary>
    /// Count the number of approved or pending (not rejected) reservations for the email.
    /// </summary>
    public Task<int> CountActiveReservationsForEmailAddress(string emailAddress);

    /// <summary>
    /// Creates a reservation. The seat must have already been locked.
    /// </summary>
    /// <remarks>
    /// WARNING: This does not verify ownership of the seat lock.
    /// </remarks>
    /// <param name="reservation">Reservation to save.</param>
    /// <returns>Id of the created reservation.</returns>
    Task<int> CreateReservation(ReservationEntityModel reservation);

    /// <summary>
    /// List all reservations.
    /// </summary>
    Task<IEnumerable<ReservationEntityModel>> ListReservations();

    /// <summary>
    /// Fetch reservation data for a reservation.
    /// </summary>
    /// <param name="reservationId">The primary key of the reservation to fetch.</param>
    /// <returns>Reservation data, null if reservation not found.</returns>
    Task<ReservationEntityModel?> FetchReservation(int reservationId);

    /// <summary>
    /// Updates reservation data.
    /// </summary>
    /// <param name="reservation">Reservation data to save.</param>
    /// <returns>True if the reservation was updated; false otherwise.</returns>
    Task<bool> UpdateReservation(ReservationEntityModel reservation);

    /// <summary>
    /// Updates the status of the reservation.
    /// </summary>
    /// <param name="reservationId">The primary key of the reservation to update.</param>
    /// <param name="reservationStatus">New status of the reservation.</param>
    /// <returns>
    /// True if successful; false if the reservation was not found.
    /// </returns>
    Task<bool> UpdateReservationStatus(int reservationId, string reservationStatus);
}

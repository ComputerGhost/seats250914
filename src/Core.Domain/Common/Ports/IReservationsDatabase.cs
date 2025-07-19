using Core.Domain.Common.Models.Entities;

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
    /// If this is called twice for the same reservation somehow,
    /// then it will return null for one of them.
    /// It checks uniqueness based on `SeatLockId`.
    /// </remarks>
    /// <param name="reservation">Reservation to save.</param>
    /// <returns>Id of the created reservation, null if failure.</returns>
    Task<int?> CreateReservation(ReservationEntityModel reservation);

    /// <summary>
    /// Deletes a reservation.
    /// </summary>
    Task DeleteReservation(int reservationId);

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

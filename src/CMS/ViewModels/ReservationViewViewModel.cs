using Core.Application.Reservations;
using Core.Domain.Common.Enumerations;

namespace CMS.ViewModels;

public class ReservationViewViewModel
{
    public ReservationViewViewModel(FetchReservationQueryResponse fetchReservationQueryResponse)
    {
        Email = fetchReservationQueryResponse.Email;
        Name = fetchReservationQueryResponse.Name;
        PhoneNumber = fetchReservationQueryResponse.PhoneNumber;
        PreferredLanguage = fetchReservationQueryResponse.PreferredLanguage;
        ReservationStatus = fetchReservationQueryResponse.Status;
        ReservedAt = fetchReservationQueryResponse.ReservedAt.ToString("yyyy-MM-ddTHH:mm:ss");
        SeatNumber = fetchReservationQueryResponse.SeatNumber;
    }

    /// <summary>
    /// Email of the person reserving the seat.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Name of the person reserving the seat.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional phone number of the person reserving the seat.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Preferred langauge of the person reserving the seat.
    /// </summary>
    public string PreferredLanguage { get; set; } = null!;

    /// <summary>
    /// When the reservation was made, formatted for display.
    /// </summary>
    public string ReservedAt { get; set; } = null!;

    /// <summary>
    /// Number of the seat reserved.
    /// </summary>
    public int SeatNumber { get; set; }

    /// <summary>
    /// Status of the reservation.
    /// </summary>
    public ReservationStatus ReservationStatus { get; set; }
}

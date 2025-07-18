using Core.Application.Reservations;
using Core.Domain.Common.Enumerations;
using Presentation.Shared.FrameworkEnhancements.Extensions;

namespace CMS.ViewModels;

public class ReservationViewViewModel
{
    public ReservationViewViewModel(int reservationId, FetchReservationQueryResponse fetchReservationQueryResponse)
    {
        Email = fetchReservationQueryResponse.Email;
        Name = fetchReservationQueryResponse.Name;
        PhoneNumber = fetchReservationQueryResponse.PhoneNumber;
        PreferredLanguage = fetchReservationQueryResponse.PreferredLanguage;
        ReservationStatus = fetchReservationQueryResponse.Status;
        ReservedAtDisplay = fetchReservationQueryResponse.ReservedAt.ToNormalizedString();
        ReservedAtParameter = fetchReservationQueryResponse.ReservedAt.ToString("s");
        SeatNumbers = string.Join(", ", fetchReservationQueryResponse.SeatNumbers);

        ReservationId = reservationId;
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
    public string ReservedAtDisplay { get; set; } = null!;

    /// <summary>
    /// When the reservation was made, formatted for time parameter.
    /// </summary>
    public string ReservedAtParameter { get; set; } = null!;

    /// <summary>
    /// Numbers of the seats reserved.
    /// </summary>
    public string SeatNumbers { get; set; } = null!;

    /// <summary>
    /// Display only. Id of the reservation.
    /// </summary>
    public int ReservationId { get; set; }

    /// <summary>
    /// Status of the reservation.
    /// </summary>
    public ReservationStatus ReservationStatus { get; set; }
}

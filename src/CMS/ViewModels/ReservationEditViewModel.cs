using Core.Application.Reservations;
using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class ReservationEditViewModel
{
    public ReservationEditViewModel()
    {
    }

    public ReservationEditViewModel(int reservationId, FetchReservationQueryResponse queryResponse)
    {
        ReservationId = reservationId;
        SeatNumber = queryResponse.SeatNumber;
        Email = queryResponse.Email;
        Name = queryResponse.Name;
        PhoneNumber = queryResponse.PhoneNumber;
        PreferredLanguage = queryResponse.PreferredLanguage;
    }

    /* Display only */

    public int ReservationId { get; set; }

    public int SeatNumber { get; set; }

    /* Form inputs */

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    public string PreferredLanguage { get; set; } = null!;

    public UpdateReservationCommand ToUpdateReservationCommand(int reservationId)
    {
        return new UpdateReservationCommand
        {
            Email = Email,
            Name = Name,
            PhoneNumber = PhoneNumber,
            PreferredLanguage = PreferredLanguage,
            ReservationId = reservationId,
            SeatNumber = SeatNumber,
        };
    }
}

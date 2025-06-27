using Core.Application.Reservations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Public.Models.ViewModels;

public class ReserveSeatViewModel
{
    /* Display only */

    [BindNever]
    public required TimeSpan TimeUntilExpiration { get; set; }

    public string TimeUntilExpirationText => TimeUntilExpiration.ToString(@"mm\:ss");

    public string TimeUntilExpirationPeriod => TimeUntilExpiration.ToString(@"\P\Tmm\Mss\S");

    [BindNever]
    public required int SeatNumber { get; set; }

    /* Form fields */

    public string Action { get; set; } = null!;

    public bool AgreeToTerms { get; set; }

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    public string PreferredLanguage { get; set; } = null!;

    public ReserveSeatCommand ToReserveSeatCommand(string ipAddress, LockSeatCommandResponse seatLock)
    {
        return new ReserveSeatCommand
        {
            IpAddress = ipAddress,
            IsStaff = false,
            Email = Email,
            Name = Name,
            PhoneNumber = PhoneNumber,
            PreferredLanguage = PreferredLanguage,
            SeatKey = seatLock.SeatKey,
            SeatNumber = seatLock.SeatNumber,
        };
    }
}

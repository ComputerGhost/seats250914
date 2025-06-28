using Core.Application.Reservations;
using Core.Application.Seats;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class ReservationCreateViewModel
{
    public ReservationCreateViewModel()
    {
    }

    public ReservationCreateViewModel(ListSeatsQueryResponse queryResponse)
    {
        ValidSeatNumbers = queryResponse.Data.Select(i => new SelectListItem
        {
            Text = i.SeatNumber.ToString(),
            Value = i.SeatNumber.ToString()
        });
    }

    /* Form options */

    public IEnumerable<SelectListItem> ValidSeatNumbers { get; init; } = null!;

    /* Form fields */

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = null!;

    public string Name { get; set; } = null!;

    [DataType(DataType.PhoneNumber)]
    public string? PhoneNumber { get; set; }

    public string PreferredLanguage { get; set; } = null!;

    public int SeatNumber { get; set; }

    public LockSeatCommand ToLockSeatCommand(string ipAddress)
    {
        return new LockSeatCommand
        {
            IpAddress = ipAddress,
            IsStaff = true,
            SeatNumber = SeatNumber,
        };
    }

    public ReserveSeatCommand ToReserveSeatCommand(string ipAddress, LockSeatCommandResponse seatLock)
    {
        return new ReserveSeatCommand
        {
            IpAddress = ipAddress,
            IsStaff = true,
            Email = Email,
            Name = Name,
            PhoneNumber = PhoneNumber,
            PreferredLanguage = PreferredLanguage,
            SeatKey = seatLock.SeatKey,
            SeatNumber = seatLock.SeatNumber,
        };
    }
}

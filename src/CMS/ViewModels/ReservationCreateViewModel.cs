using Core.Application.Reservations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CMS.ViewModels;

public class ReservationCreateViewModel
{
    public ReservationCreateViewModel()
    {
    }

    /* Form options */

    public IEnumerable<SelectListItem> ValidSeatNumbers => Enumerable.Range(1, 100)
        .Select(i => new SelectListItem { Text = i.ToString(), Value = i.ToString(), });

    /* Form fields */

    [MaxLength(255)]
    [Required]
    public string Email { get; set; } = null!;

    [MaxLength(50)]
    [Required]
    public string Name { get; set; } = null!;

    [DataType(DataType.PhoneNumber)]
    [MaxLength(15)]
    public string? PhoneNumber { get; set; }

    [MaxLength(50)]
    [Required]
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

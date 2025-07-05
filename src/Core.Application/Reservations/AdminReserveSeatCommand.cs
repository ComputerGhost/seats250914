using Core.Domain.Common.Models;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;

/// <summary>
/// Attempts to make a reservation. Returns the reservation id if successful.
/// </summary>
public class AdminReserveSeatCommand : IRequest<ErrorOr<int>>
{
    // We don't use the IP if it's from the admin.
    internal readonly string IpAddress = "-";

    /// <summary>
    /// The number identifier of the seat to reserve.
    /// </summary>
    public int SeatNumber { get; set; }

    /// <summary>
    /// Name of the person reserving the seat.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Email of the person reserving the seat.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Optional phone number of the person reserving the seat.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Preferred langauge of the person reserving the seat.
    /// </summary>
    public string PreferredLanguage { get; set; } = null!;

    internal IdentityModel Identity => new()
    {
        Email = Email,
        IsStaff = true,
        IpAddress = IpAddress,
        Name = Name,
        PhoneNumber = PhoneNumber,
        PreferredLanguage = PreferredLanguage,
    };
}

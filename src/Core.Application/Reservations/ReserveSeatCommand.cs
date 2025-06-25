using Core.Domain.Common.Models;
using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;

/// <summary>
/// Attempts to make a reservation. Returns the reservation id if successful.
/// </summary>
/// <remarks>
/// A lock is required to reserve a seat.
/// See <see cref="LockSeatCommand"/> for how to lock a seat.
/// </remarks>
public class ReserveSeatCommand : IRequest<ErrorOr<int>>
{
    /// <summary>
    /// Ip address of the one reserving the seat.
    /// </summary>
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// Whether the seat is being reserved by staff instead of by the user.
    /// </summary>
    public bool IsStaff { get; set; }

    /// <summary>
    /// The number identifier of the seat to reserve.
    /// </summary>
    public int SeatNumber { get; set; }

    /// <summary>
    /// Key to authorize reserving the seat.
    /// </summary>
    public string SeatKey { get; set; } = null!;

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
        IsStaff = IsStaff,
        IpAddress = IpAddress,
        Name = Name,
        PhoneNumber = PhoneNumber,
        PreferredLanguage = PreferredLanguage,
    };
}

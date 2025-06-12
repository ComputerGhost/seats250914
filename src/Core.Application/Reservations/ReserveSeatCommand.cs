using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;

/// <summary>
/// Attempts to make a reservation.
/// </summary>
/// <remarks>
/// A lock is required to reserve a seat.
/// See <see cref="LockSeatCommand"/> for how to lock a seat.
/// </remarks>
public class ReserveSeatCommand : IRequest<ErrorOr<Success>>
{
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
}

using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;

/// <summary>
/// Attempts to lock a seat.
/// </summary>
/// <remarks>
/// If two people simultaneously reserve a seat, only one will be successful.
/// An error result will be returned for the losing person.
/// </remarks>
public class LockSeatCommand : IRequest<ErrorOr<LockSeatCommandResponse>>
{
    /// <summary>
    /// Ip address of the one reserving the seat.
    /// </summary>
    public string IpAddress { get; set; } = null!;

    /// <summary>
    /// Whether the seat is being locked by staff instead of by the user.
    /// </summary>
    public bool IsStaff { get; set; }

    /// <summary>
    /// The number of the seat to be locked.
    /// </summary>
    public int SeatNumber { get; set; }
}

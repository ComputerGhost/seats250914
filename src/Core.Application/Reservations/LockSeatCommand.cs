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
    public int SeatNumber { get; set; }
}

using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
public class UnlockSeatCommand : IRequest<ErrorOr<Success>>
{
    /// <summary>
    /// Key to authorize unlocking the seat.
    /// </summary>
    public required string SeatKey { get; set; } = null!;

    /// <summary>
    /// The number identifier of the seat to unlock.
    /// </summary>
    public required int SeatNumber { get; set; }
}

using ErrorOr;
using MediatR;

namespace Core.Application.Reservations;
public class UnlockSeatsCommand : IRequest<ErrorOr<Success>>
{
    /// <summary>
    /// Map of locked seats to their keys.
    /// </summary>
    public IDictionary<int, string> SeatLocks { get; set; } = null!;
}

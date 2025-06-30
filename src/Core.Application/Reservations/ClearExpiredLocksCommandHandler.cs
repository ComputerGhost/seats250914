using Core.Domain.Reservations;
using MediatR;
using Serilog;

namespace Core.Application.Reservations;
internal class ClearExpiredLocksCommandHandler(ISeatLockService seatLockService)
    : IRequestHandler<ClearExpiredLocksCommand>
{
    public Task Handle(ClearExpiredLocksCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Clearing expired logs.");
        return seatLockService.ClearExpiredLocks();
    }
}

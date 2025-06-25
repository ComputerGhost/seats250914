using Core.Domain.Reservations;
using MediatR;

namespace Core.Application.Reservations;
internal class ClearExpiredLocksCommandHandler(ISeatLockService seatLockService) : IRequestHandler<ClearExpiredLocksCommand>
{
    public Task Handle(ClearExpiredLocksCommand request, CancellationToken cancellationToken)
    {
        return seatLockService.ClearExpiredLocks();
    }
}

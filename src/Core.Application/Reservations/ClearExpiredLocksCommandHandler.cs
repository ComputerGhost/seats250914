using Core.Domain.Common.Ports;
using MediatR;

namespace Core.Application.Reservations;
internal class ClearExpiredLocksCommandHandler : IRequestHandler<ClearExpiredLocksCommand>
{
    private readonly ISeatLocksDatabase _seatLocksDatabase;

    public ClearExpiredLocksCommandHandler(ISeatLocksDatabase seatLocksDatabase)
    {
        _seatLocksDatabase = seatLocksDatabase;
    }

    public Task Handle(ClearExpiredLocksCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return _seatLocksDatabase.ClearExpiredLocks(now);
    }
}

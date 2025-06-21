using Core.Domain.Common.Ports;
using MediatR;

namespace Core.Application.Reservations;
internal class ClearExpiredLocksCommandHandler : IRequestHandler<ClearExpiredLocksCommand>
{
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public ClearExpiredLocksCommandHandler(ISeatLocksDatabase seatLocksDatabase, ISeatsDatabase seatsDatabase)
    {
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task Handle(ClearExpiredLocksCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        await _seatLocksDatabase.ClearExpiredLocks(now);
        await _seatsDatabase.ResetLockedSeatStatuses();
    }
}

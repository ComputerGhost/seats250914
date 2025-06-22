using Core.Domain.Common.Ports;
using MediatR;

namespace Core.Application.Reservations;
internal class ClearExpiredLocksCommandHandler : IRequestHandler<ClearExpiredLocksCommand>
{
    private readonly IConfigurationDatabase _configurationDatabase;
    private readonly ISeatLocksDatabase _seatLocksDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public ClearExpiredLocksCommandHandler(
        IConfigurationDatabase configurationDatabase,
        ISeatLocksDatabase seatLocksDatabase, 
        ISeatsDatabase seatsDatabase)
    {
        _configurationDatabase = configurationDatabase;
        _seatLocksDatabase = seatLocksDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task Handle(ClearExpiredLocksCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow + await FetchGracePeriod();
        await _seatLocksDatabase.ClearExpiredLocks(now);
        await _seatsDatabase.ResetUnlockedSeatStatuses();
    }

    private async Task<TimeSpan> FetchGracePeriod()
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        return TimeSpan.FromSeconds(configuration.GracePeriodSeconds);
    }
}

using Core.Domain.Common.Ports;
using Core.Domain.Scheduling;
using MediatR;
using Serilog;

namespace Core.Application.System;
internal class FetchReservationsStatusQueryHandler : IRequestHandler<FetchReservationsStatusQuery, FetchReservationsStatusQueryResponse>
{
    private readonly IConfigurationDatabase _configurationDatabase;
    private readonly ISeatsDatabase _seatsDatabase;

    public FetchReservationsStatusQueryHandler(IConfigurationDatabase configurationDatabase, ISeatsDatabase seatsDatabase)
    {
        _configurationDatabase = configurationDatabase;
        _seatsDatabase = seatsDatabase;
    }

    public async Task<FetchReservationsStatusQueryResponse> Handle(FetchReservationsStatusQuery request, CancellationToken cancellationToken)
    {
        Log.Information("Fetching reservations system status.");

        var openChecker = await OpenChecker.FromDatabase(_configurationDatabase, _seatsDatabase);

        var response = new FetchReservationsStatusQueryResponse
        {
            Status = await openChecker.CalculateStatus(),
            ScheduledCloseDateTime = openChecker.ScheduledCloseDateTime,
            ScheduledCloseTimeZone = openChecker.ScheduledCloseTimeZone,
            ScheduledOpenDateTime = openChecker.ScheduledOpenDateTime,
            ScheduledOpenTimeZone = openChecker.ScheduledOpenTimeZone,
        };

        Log.Debug("Reservations system status is {@response}.", response);
        return response;
    }
}

using Core.Domain.Common.Ports;
using Core.Domain.Scheduling;
using MediatR;
using Serilog;

namespace Core.Application.System;
internal class FetchConfigurationQueryHandler(IConfigurationDatabase configurationDatabase, ISeatsDatabase seatsDatabase)
    : IRequestHandler<FetchConfigurationQuery, FetchConfigurationQueryResponse>
{
    public async Task<FetchConfigurationQueryResponse> Handle(FetchConfigurationQuery request, CancellationToken cancellationToken)
    {
        Log.Information("Fetching configuration.");
        var configurationEntity = await configurationDatabase.FetchConfiguration();
        var openChecker = OpenChecker.FromConfiguration(configurationEntity, seatsDatabase);
        return new FetchConfigurationQueryResponse(configurationEntity)
        {
            AreReservationsOpen = openChecker.AreReservationsOpen(),
            ReservationsStatus = await openChecker.CalculateStatus(),
        };
    }
}

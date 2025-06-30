using Core.Domain.Common.Ports;
using MediatR;
using Serilog;

namespace Core.Application.System;
internal class FetchConfigurationQueryHandler(IConfigurationDatabase configurationDatabase)
    : IRequestHandler<FetchConfigurationQuery, FetchConfigurationQueryResponse>
{
    public async Task<FetchConfigurationQueryResponse> Handle(FetchConfigurationQuery request, CancellationToken cancellationToken)
    {
        Log.Information("Fetching configuration.");
        var configurationEntity = await configurationDatabase.FetchConfiguration();
        return new FetchConfigurationQueryResponse(configurationEntity);
    }
}

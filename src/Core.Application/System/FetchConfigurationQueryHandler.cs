using Core.Domain.Common.Ports;
using MediatR;

namespace Core.Application.System;
internal class FetchConfigurationQueryHandler(IConfigurationDatabase configurationDatabase) : IRequestHandler<FetchConfigurationQuery, FetchConfigurationQueryResponse>
{
    public async Task<FetchConfigurationQueryResponse> Handle(FetchConfigurationQuery request, CancellationToken cancellationToken)
    {
        var configurationEntity = await configurationDatabase.FetchConfiguration();
        return new FetchConfigurationQueryResponse(configurationEntity);
    }
}

using Core.Domain.Common.Ports;
using MediatR;
using Serilog;

namespace Core.Application.System;
internal class SaveConfigurationHandler(IConfigurationDatabase configurationDatabase)
    : IRequestHandler<SaveConfigurationCommand>
{
    public async Task Handle(SaveConfigurationCommand request, CancellationToken cancellationToken)
    {
        Log.Information("Saving system configuration.");
        Log.Debug("Configuration data being saved is: {@request}", request);

        var entityModel = request.ToConfigurationEntityModel();
        if (!await configurationDatabase.SaveConfiguration(entityModel))
        {
            Log.Error("Saving the configuration failed unexpectedly.");
            throw new Exception("Saving the configuration failed unexpectedly.");
        }
    }
}

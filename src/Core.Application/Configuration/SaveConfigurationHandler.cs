using Core.Domain.Common.Ports;
using MediatR;

namespace Core.Application.Configuration;
internal class SaveConfigurationHandler : IRequestHandler<SaveConfigurationCommand>
{
    private readonly IConfigurationDatabase _configurationDatabase;

    public SaveConfigurationHandler(IConfigurationDatabase configurationDatabase)
    {
        _configurationDatabase = configurationDatabase;
    }

    public async Task Handle(SaveConfigurationCommand request, CancellationToken cancellationToken)
    {
        var entityModel = request.ToConfigurationEntityModel();
        if (!await _configurationDatabase.SaveConfiguration(entityModel))
        {
            throw new Exception("Saving the configuration failed unexpectedly.");
        }
    }
}

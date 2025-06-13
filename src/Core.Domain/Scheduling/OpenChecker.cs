using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;

namespace Core.Domain.Scheduling;
public class OpenChecker
{
    private readonly IConfigurationDatabase _configurationDatabase;

    public OpenChecker(IConfigurationDatabase configurationDatabase)
    {
        _configurationDatabase = configurationDatabase;
    }

    public async Task<bool> AreReservationsOpen()
    {
        var configuration = await _configurationDatabase.FetchConfiguration();
        return AreReservationsOpen(configuration);
    }

    public static bool AreReservationsOpen(ConfigurationEntityModel configuration)
    {
        if (configuration.ForceCloseReservations || configuration.ForceOpenReservations)
        {
            return configuration.ForceOpenReservations;
        }

        var now = DateTime.UtcNow;
        return configuration.ScheduledOpenDateTime < now;
    }
}

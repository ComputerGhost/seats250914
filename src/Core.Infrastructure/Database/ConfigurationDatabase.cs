using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Core.Infrastructure.Adapters;

[ServiceImplementation(Lifetime = ServiceLifetime.Scoped)]
internal class ConfigurationDatabase(IDbConnection connection) : IConfigurationDatabase
{
    // We query the config a few times per request, so save this for the current scope.
    private ConfigurationEntityModel? _cachedConfiguration = null;

    public async Task<ConfigurationEntityModel> FetchConfiguration()
    {
        var cachedConfiguration = _cachedConfiguration;
        if (cachedConfiguration == null)
        {
            var sql = "SELECT TOP 1 * FROM Configuration ORDER BY DateSaved DESC";
            _cachedConfiguration = await connection.QueryFirstOrDefaultAsync<ConfigurationEntityModel>(sql)
                ?? ConfigurationEntityModel.Default;
            cachedConfiguration = _cachedConfiguration;
        }

        return cachedConfiguration;
    }

    public async Task<bool> SaveConfiguration(ConfigurationEntityModel configuration)
    {
        _cachedConfiguration = null;

        var sql = """
            INSERT INTO Configuration (ForceCloseReservations, ForceOpenReservations, GracePeriodSeconds, MaxSeatsPerPerson, MaxSeatsPerIPAddress, MaxSeatsPerReservation, MaxSecondsToConfirmSeat, ScheduledOpenDateTime, ScheduledOpenTimeZone, ScheduledCloseDateTime, ScheduledCloseTimeZone)
            VALUES (@ForceCloseReservations, @ForceOpenReservations, @GracePeriodSeconds, @MaxSeatsPerPerson, @MaxSeatsPerIPAddress, @MaxSeatsPerReservation, @MaxSecondsToConfirmSeat, @ScheduledOpenDateTime, @ScheduledOpenTimeZone, @ScheduledCloseDateTime, @ScheduledCloseTimeZone);
            """;
        return await connection.ExecuteAsync(sql, configuration) > 0;
    }
}

using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using System.Data;

namespace Core.Infrastructure.Adapters;

[ServiceImplementation]
internal class ConfigurationDatabase(IDbConnection connection) : IConfigurationDatabase
{
    public async Task<ConfigurationEntityModel> FetchConfiguration()
    {
        var sql = "SELECT TOP 1 * FROM Configuration ORDER BY DateSaved DESC";
        return await connection.QueryFirstOrDefaultAsync<ConfigurationEntityModel>(sql)
            ?? ConfigurationEntityModel.Default;
    }

    public async Task<bool> SaveConfiguration(ConfigurationEntityModel configuration)
    {
        var sql = """
            INSERT INTO Configuration (ForceCloseReservations, ForceOpenReservations, GracePeriodSeconds, MaxSeatsPerPerson, MaxSeatsPerIPAddress, MaxSecondsToConfirmSeat, ScheduledOpenDateTime, ScheduledOpenTimeZone)
            VALUES (@ForceCloseReservations, @ForceOpenReservations, @GracePeriodSeconds, @MaxSeatsPerPerson, @MaxSeatsPerIPAddress, @MaxSecondsToConfirmSeat, @ScheduledOpenDateTime, @ScheduledOpenTimeZone);
            """;
        return await connection.ExecuteAsync(sql, configuration) > 0;
    }
}

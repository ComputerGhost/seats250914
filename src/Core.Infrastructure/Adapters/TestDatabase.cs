using Core.Domain.Common.Ports;
using Core.Domain.DependencyInjection;
using Dapper;
using System.Data;

namespace Core.Infrastructure.Adapters;

[ServiceImplementation]
internal class TestDatabase : ITestDatabase
{
    private readonly IDbConnection _connection;

    public TestDatabase(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task PingDatabase()
    {
        await _connection.ExecuteAsync("SELECT 1");
    }
}

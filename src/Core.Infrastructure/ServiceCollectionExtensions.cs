using Core.Domain.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Data;
using System.Reflection;

namespace Core.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<InfrastructureOptions> configure)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services
            .Configure(configure)
            .AddTransient(CreateDatabaseConnection)
            .AddServiceImplementations(assembly);
    }

    private static IDbConnection CreateDatabaseConnection(IServiceProvider provider)
    {
        var configuration = provider.GetRequiredService<IOptions<InfrastructureOptions>>();
        var connectionString = configuration.Value.DatabaseConnectionString;
        return new SqlConnection(connectionString);
    }
}

using DatabaseMigrator.Models;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var config = LoadConfiguration();
using var serviceProvider = CreateServices(config);
using var scope = serviceProvider.CreateScope();
UpdateDatabase(scope.ServiceProvider);

static ServiceProvider CreateServices(ConfigModel config)
{
    return new ServiceCollection()
        .AddFluentMigratorCore()
        .ConfigureRunner(builder => builder
            .AddSqlServer()
            .WithGlobalConnectionString(config.InfrastructureOptions.DatabaseConnectionString)
            .ScanIn(Assembly.GetExecutingAssembly())
        )
        .AddLogging(builder => builder.AddFluentMigratorConsole())
        .BuildServiceProvider(false);
}

static ConfigModel LoadConfiguration()
{
    var configurationManager = new ConfigurationManager();
    configurationManager
        .AddJsonFile("appsettings.json")
        .AddEnvironmentVariables();
    return configurationManager.Get<ConfigModel>()
        ?? throw new Exception("Config file format is invalid.");
}

static void UpdateDatabase(IServiceProvider serviceProvider)
{
    var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}


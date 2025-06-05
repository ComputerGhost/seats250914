using Core.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.IntegrationTests;

/// <summary>
/// Minimal application that uses Core.Application.
/// </summary>
internal class MinimalApplication
{
    public static MinimalApplication Create()
    {
        return new();
    }

    private MinimalApplication()
    {
        ConfigurationManager = new ConfigurationManager();
        ConfigurationManager
            .AddJsonFile("testsettings.json")
            .AddEnvironmentVariables();

        ServiceCollection = new ServiceCollection();
        ServiceCollection.AddCore(options =>
        {
            ConfigurationManager.Bind("InfrastructureOptions", options);
        });

        ServiceProvider = ServiceCollection.BuildServiceProvider();
    }

    public IServiceCollection ServiceCollection { get; }
    public IServiceProvider ServiceProvider { get; }
    public ConfigurationManager ConfigurationManager { get; }
}

using Core.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMS.FrontendIntegrationTests;
internal class ConfigurationAccessor
{
    private static readonly Lock _lock = new();
    private static ConfigurationAccessor? _instance = null;

    private ConfigurationAccessor()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager
            .AddJsonFile("testsettings.json")
            .AddEnvironmentVariables();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCore(options =>
        {
            configurationManager.Bind("InfrastructureOptions", options);
        });

        TargetUrl = configurationManager["targetUrl"]!;

        Services = serviceCollection.BuildServiceProvider();
    }

    public IServiceProvider Services { get; set; }
    public string TargetUrl { get; set; }

    public static ConfigurationAccessor Instance
    {
        get
        {
            lock (_lock)
            {
                return _instance ??= new ConfigurationAccessor();
            }
        }
    }
}

using Core.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CMS.IntegrationTests;
internal class ConfigurationAccessor
{
    private static readonly Lock _lock = new();
    private static ConfigurationAccessor? _instance = null;

    private ConfigurationAccessor()
    {
        var configurationManager = new ConfigurationManager();
        configurationManager.AddJsonFile("testsettings.json");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddCore(options =>
        {
            configurationManager.Bind("InfrastructureOptions", options);
        });

        TargetUrl = configurationManager["targetUrl"]!;
        Username = configurationManager["username"]!;
        Password = configurationManager["password"]!;

        Services = serviceCollection.BuildServiceProvider();
    }

    public IServiceProvider Services { get; set; }
    public string TargetUrl { get; private set; }
    public string Username { get; private set; }
    public string Password { get; private set; }

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

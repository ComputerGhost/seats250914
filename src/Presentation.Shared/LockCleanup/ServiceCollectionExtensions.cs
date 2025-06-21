using Microsoft.Extensions.DependencyInjection;

namespace Presentation.Shared.LockCleanup;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCleanupScheduler(this IServiceCollection services, Action<CleanupOptions>? configure = null)
    {
        var options = new CleanupOptions();
        configure?.Invoke(options);

        services.AddHostedService((provider) => new CleanupScheduler(provider)
        {
            MaxWaitSeconds = options.MaxWaitSeconds,
        });

        return services;
    }
}

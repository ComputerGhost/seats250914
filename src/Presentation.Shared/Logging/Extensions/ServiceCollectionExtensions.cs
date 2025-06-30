using Microsoft.Extensions.DependencyInjection;
using Presentation.Shared.Logging.Enrichers;
using Presentation.Shared.Logging.Models;
using Serilog;
using Serilog.Events;

namespace Presentation.Shared.Logging.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMyLogging(this IServiceCollection services, Action<LoggingOptions> configure)
    {
        var options = new LoggingOptions();
        configure(options);

        const string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties}{NewLine}{Exception}";

        var loggerConfiguration = new LoggerConfiguration()
            .Enrich.WithThreadId()
            .Enrich.With<UserNameEnricher>()
            .MinimumLevel.Debug()
            .WriteTo.Console(outputTemplate: outputTemplate);

        if (options.LogDirectory != null)
        {
            loggerConfiguration.WriteTo.File(
                Path.Combine(options.LogDirectory, "log-.txt"),
                outputTemplate: outputTemplate,
                restrictedToMinimumLevel: LogEventLevel.Information,
                retainedFileCountLimit: 10,
                rollingInterval: RollingInterval.Day
            );
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        services.AddHttpContextAccessor();

        return services;
    }
}

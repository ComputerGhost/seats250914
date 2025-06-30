using Microsoft.Extensions.DependencyInjection;
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

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console();

        if (options.LogDirectory != null)
        {
            loggerConfiguration.WriteTo.File(
                Path.Combine(options.LogDirectory, "log-.txt"),
                restrictedToMinimumLevel: LogEventLevel.Information,
                retainedFileCountLimit: 10,
                rollingInterval: RollingInterval.Day
            );
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        return services;
    }
}

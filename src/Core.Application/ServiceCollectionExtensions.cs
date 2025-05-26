using Core.Infrastructure.MockData;
using MediatR.Behaviors.Authorization.Extensions.DependencyInjection;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Application;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCore(this IServiceCollection services, Action<InfrastructureOptions> configure)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services
            .AddMediatR(assembly)
            .AddInfrastructure(configure);
    }

    private static IServiceCollection AddMediatR(this IServiceCollection services, Assembly assembly)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(assembly);
        });

        services.AddFluentValidation([assembly]);

        services.AddMediatorAuthorization(assembly);
        services.AddAuthorizersFromAssembly(assembly);

        return services;
    }
}

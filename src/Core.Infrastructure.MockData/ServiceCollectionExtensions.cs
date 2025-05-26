using Core.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Infrastructure.MockData;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<InfrastructureOptions> configure)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services
            .Configure(configure)
            .AddServiceImplementations(assembly);
    }
}

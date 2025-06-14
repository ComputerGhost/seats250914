using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Domain.DependencyInjection;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        return services.AddServiceImplementations(assembly);
    }

    public static IServiceCollection AddServiceImplementations(this IServiceCollection services, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var attribute in type.GetCustomAttributes<ServiceImplementationAttribute>())
            {
                services.Add(new ServiceDescriptor(attribute.GetInterface(type), type, attribute.Lifetime));
            }
        }

        return services;
    }
}

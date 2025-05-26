using Core.Application;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.IntegrationTests;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void Startup_WhenComplete_AllServicesAreImplemented()
    {
        // Arrange
        var services = new ServiceCollection();
        var dependencies = GetQueryHandlerTypes()
            .SelectMany(GetConstructorParametersTypes)
            .Distinct();

        // Act
        services.AddCore((_) => { });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        foreach (var dependency in dependencies)
        {
            var resolvedService = serviceProvider.GetService(dependency);
            Assert.IsNotNull(resolvedService);
        }
    }

    private IEnumerable<Type> GetQueryHandlerTypes()
    {
        var applicationAssembly = Assembly.GetAssembly(typeof(Reference))!;

        return new ServiceCollection()
            .AddMediatR(config => config.RegisterServicesFromAssembly(applicationAssembly))
            .Where(x => x.ImplementationType?.Assembly == applicationAssembly)
            .Select(x => x.ImplementationType!);
    }

    private IEnumerable<Type> GetConstructorParametersTypes(Type queryHandler)
    {
        var constructor = queryHandler.GetConstructors().FirstOrDefault();
        if (constructor == null)
        {
            return [];
        }

        return constructor.GetParameters().Select(x => x.ParameterType);
    }
}

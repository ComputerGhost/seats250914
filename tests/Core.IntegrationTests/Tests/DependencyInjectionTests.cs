using Core.Application;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.IntegrationTests.Tests;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void Startup_WhenComplete_AllServicesAreImplemented()
    {
        // Arrange
        var dependencies = GetQueryHandlerTypes()
            .SelectMany(GetConstructorParametersTypes)
            .Distinct();

        // Act
        var app = MinimalApplication.Create();

        // Assert
        foreach (var dependency in dependencies)
        {
            var resolvedService = app.ServiceProvider.GetService(dependency);
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

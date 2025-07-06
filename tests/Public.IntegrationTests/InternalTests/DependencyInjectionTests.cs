using Microsoft.Extensions.DependencyInjection;
using Public.IntegrationTests;
using System.Reflection;
using Reference = Public.Controllers.HomeController;

namespace Public.IntegrationTests.InternalTests;

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
        var app = ConfigurationAccessor.Instance;

        // Assert
        foreach (var dependency in dependencies)
        {
            var resolvedService = app.Services.GetService(dependency);
            Assert.IsNotNull(resolvedService, $"Could not load dependency {dependency}");
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

        return constructor
            .GetParameters()
            .Where(p => !p.IsOptional)
            .Select(p => p.ParameterType);
    }
}

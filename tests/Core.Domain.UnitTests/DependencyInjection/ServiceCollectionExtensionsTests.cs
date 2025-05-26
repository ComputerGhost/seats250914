using Core.Domain.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Domain.UnitTests.DependencyInjection;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    // Cached so we don't have to keep assembly scanning.
    private static IServiceProvider? _cachedServiceProvider;
    private IServiceProvider _serviceProvider = null!;

    [TestInitialize]
    public void Initialize()
    {
        _cachedServiceProvider ??= new ServiceCollection()
            .AddServiceImplementations(Assembly.GetExecutingAssembly())
            .BuildServiceProvider();
        _serviceProvider = _cachedServiceProvider;
    }

    [TestMethod]
    public void WhenNotMarkedAsImplementation_DoesNotRegisterService()
    {
        // Arrange
        var serviceType = typeof(TestServices.IUnregistered);

        // Act
        var implementation = _serviceProvider.GetService(serviceType);

        // Assert
        Assert.IsNull(implementation);
    }

    [TestMethod]
    public void WhenImplementsOneService_RegistersService()
    {
        // Arrange
        var serviceType = typeof(TestServices.IRegistered);
        var implementationType = typeof(TestServices.Registered);

        // Act
        var implementation = _serviceProvider.GetService(serviceType);

        // Assert
        Assert.IsInstanceOfType(implementation, implementationType);
    }

    [TestMethod]
    public void WhenImplementsTwoServices_RegistersBoth()
    {
        // Arrange
        var firstServiceType = typeof(TestServices.IFirst);
        var secondServiceType = typeof(TestServices.ISecond);
        var implementationType = typeof(TestServices.RegisteredTwice);

        // Act
        var firstImplementation = _serviceProvider.GetService(firstServiceType);
        var secondImplementation = _serviceProvider.GetService(secondServiceType);

        // Assert
        Assert.IsInstanceOfType(firstImplementation, implementationType);
        Assert.IsInstanceOfType(secondImplementation, implementationType);
    }

    private struct TestServices
    {
        public interface IUnregistered { };
        public class Unregistered : IUnregistered { }

        public interface IRegistered { }
        [ServiceImplementation]
        public class Registered : IRegistered { }

        public interface IFirst { }
        public interface ISecond { }
        [ServiceImplementation(Interface = typeof(IFirst))]
        [ServiceImplementation(Interface = typeof(ISecond))]
        public class RegisteredTwice : IFirst, ISecond { }
    }
}

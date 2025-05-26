using Core.Domain.DependencyInjection;

namespace Core.Domain.UnitTests.DependencyInjection;

[TestClass]
public class ServiceImplementationAttributeTests
{
    private interface Interface1 { }
    private interface Interface2 { }
    private class OneInterface : Interface1 { }
    private class TwoInterfaces : Interface1, Interface2 { }

    [TestMethod]
    public void WhenInterfaceNotSpecified_AndHasOneInterface_DeducesInterface()
    {
        // Arrange
        var attribute = new ServiceImplementationAttribute();
        var implementation = typeof(OneInterface);

        // Act
        var @interface = attribute.GetInterface(implementation);

        // Assert
        Assert.AreEqual(@interface, typeof(Interface1));
    }

    [TestMethod]
    public void WhenInterfaceNotSpecified_AndHasTwoInterfaces_ThrowsException()
    {
        // Arrange
        var attribute = new ServiceImplementationAttribute();
        var implementation = typeof(TwoInterfaces);

        // Act
        var action = () => attribute.GetInterface(implementation);

        // Assert
        Assert.ThrowsException<ArgumentException>(action);
    }

    [TestMethod]
    public void WhenInterfaceSpecified_AndHasTwoInterfaces_UsesSpecifiedInterface()
    {
        // Arrange
        var expectedInterface = typeof(Interface1);
        var attribute = new ServiceImplementationAttribute { Interface = expectedInterface };
        var implementation = typeof(TwoInterfaces);

        // Act
        var actualInterface = attribute.GetInterface(implementation);

        // Assert
        Assert.AreEqual(expectedInterface, actualInterface);
    }
}

using Core.Application.Reservations;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class LockSeatCommandValidatorTests
{
    private LockSeatCommand Command { get; set; } = null!;
    private LockSeatCommandValidator Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Command = new LockSeatCommand
        {
            SeatNumber = 1,
            IpAddress = "ip address",
        };

        Subject = new();
    }

    [TestMethod]
    public void IpAddress_WhenNotEmpty_Passes()
    {
        // Arrange
        Command.IpAddress = "ip address";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void IpAddress_WhenEmpty_Fails()
    {
        // Arrange
        Command.IpAddress = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void IpAddress_WhenNull_fails()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.IpAddress = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void IpAddress_WhenTooLong_Fails()
    {
        // Arrange
        const int MAX_LENGTH = 45;
        Command.IpAddress = new string('a', MAX_LENGTH + 1);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

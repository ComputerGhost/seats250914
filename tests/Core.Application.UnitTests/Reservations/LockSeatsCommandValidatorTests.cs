using Core.Application.Reservations;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class LockSeatsCommandValidatorTests
{
    private LockSeatsCommand Command { get; set; } = null!;
    private LockSeatsCommandValidator Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Command = new LockSeatsCommand
        {
            SeatNumbers = [1],
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

    public void SeatNumbers_WhenNotEmpty_Passes()
    {
        // Arrange
        Command.SeatNumbers = [1];

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    public void SeatNumbers_WhenEmpty_Fails()
    {
        // Arrange
        Command.SeatNumbers = [];

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    public void SeatNumbers_WhenNull_Fails()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.SeatNumbers = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

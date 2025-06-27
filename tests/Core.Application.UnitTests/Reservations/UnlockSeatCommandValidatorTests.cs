using Core.Application.Reservations;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class UnlockSeatCommandValidatorTests
{
    private UnlockSeatCommand Command { get; set; } = null!;
    private UnlockSeatCommandValidator Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Command = new UnlockSeatCommand
        {
            SeatKey = new string('a', 44),
            SeatNumber = 1,
        };

        Subject = new();
    }

    [TestMethod]
    public void SeatKey_WhenCorrectLength_Passes()
    {
        // Arrange
        const int CORRECT_LENGTH = 44;
        Command.SeatKey = new string('a', CORRECT_LENGTH);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [DataTestMethod]
    [DataRow(43)]
    [DataRow(45)]
    public void SeatKey_WhenNotCorrectLength_Fails(int length)
    {
        // Precondition
        const int CORRECT_LENGTH = 44;
        Assert.AreNotEqual(CORRECT_LENGTH, length);

        // Arrange
        Command.SeatKey = new string('a', length);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void SeatKey_WhenEmpty_Fails()
    {
        // Arrange
        Command.SeatKey = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void SeatKey_WhenNull_Fails()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.SeatKey = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

using Core.Application.Reservations;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class UnlockSeatsCommandValidatorTests
{
    private UnlockSeatsCommand Command { get; set; } = null!;
    private UnlockSeatsCommandValidator Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Command = new UnlockSeatsCommand
        {
            SeatLocks = new Dictionary<int, string>() { { 1, "" } },
        };

        Subject = new();
    }

    [TestMethod]
    public void SeatKey_WhenEmpty_Fails()
    {
        // Arrange
        Command.SeatLocks.Clear();

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
        Command.SeatLocks = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

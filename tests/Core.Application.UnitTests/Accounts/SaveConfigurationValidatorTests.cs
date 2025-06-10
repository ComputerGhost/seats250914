using Core.Application.Configuration;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class SaveConfigurationValidatorTests
{
    private SaveConfigurationCommand Command { get; set; } = null!;
    private SaveConfigurationValidator Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Command = new SaveConfigurationCommand
        {
            ScheduledOpenTimeZone = "UTC",
        };

        Subject = new();
    }

    [TestMethod]
    public void Model_WhenValid_Passes()
    {
        // Arrange
        Command.ScheduledOpenTimeZone = "UTC";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [DataTestMethod]
    [DataRow(false, false, true)]
    [DataRow(true, false, true)]
    [DataRow(false, true, true)]
    [DataRow(true, true, false)]
    public void ForceOpenReservations_And_ForceCloseReservations_CannotBothBeEnabled(bool open, bool close, bool pass)
    {
        // Arrange
        Command.ForceOpenReservations = open;
        Command.ForceCloseReservations = close;

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.AreEqual(pass, result.IsValid);
    }

    [TestMethod]
    public void ScheduledOpenTimeZone_WhenInvalid_Fails()
    {
        // Arrange
        Command.ScheduledOpenTimeZone = "invalid";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void ScheduledOpenTimeZone_WhenEmpty_Fails()
    {
        // Arrange
        Command.ScheduledOpenTimeZone = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

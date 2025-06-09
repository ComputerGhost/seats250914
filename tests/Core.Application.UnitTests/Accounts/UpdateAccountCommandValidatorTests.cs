using Core.Application.Accounts;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class UpdateAccountCommandValidatorTests
{
    private UpdateAccountCommand Command { get; set; } = null!;
    private UpdateAccountCommandValidator Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Command = new UpdateAccountCommand
        {
            Login = "Valid",
            IsEnabled = true,
        };

        Subject = new();
    }

    [TestMethod]
    public void Login_WhenNotEmpty_Passes()
    {
        // Arrange
        Command.Login = "Valid";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Login_WhenEmpty_Fails()
    {
        // Arrange
        Command.Login = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Login_WhenNull_Fails()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.Login = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

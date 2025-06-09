using Core.Application.Accounts;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class UpdatePasswordCommandValidatorTests
{
    private UpdatePasswordCommand Command { get; set; } = null!;
    private UpdatePasswordCommandValidator Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Command = new UpdatePasswordCommand
        {
            Login = "Valid login",
            Password = "Valid password",
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

    [TestMethod]
    public void Password_WhenNotEmpty_Passes()
    {
        // Arrange
        Command.Password = "Not empty";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Password_WhenEmpty_Fails()
    {
        // Arrange
        Command.Password = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Password_WhenNull_Fails()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.Password = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

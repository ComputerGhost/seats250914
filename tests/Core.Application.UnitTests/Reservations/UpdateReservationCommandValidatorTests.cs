using Core.Application.Reservations;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class UpdateReservationCommandValidatorTests
{
    private UpdateReservationCommand Command { get; set; } = null!;
    private UpdateReservationCommandValidator Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Command = new UpdateReservationCommand
        {
            Name = "name",
            Email = "email@email.com",
            PreferredLanguage = "English",
        };

        Subject = new();
    }

    [TestMethod]
    public void Name_WhenNotEmpty_Passes()
    {
        // Arrange
        Command.Name = "Valid";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Name_WhenEmpty_Fails()
    {
        // Arrange
        Command.Name = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Name_WhenNull_Fails()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.Name = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Name_WhenTooLong_Fails()
    {
        // Arrange
        const int LENGTH = 100;
        Command.Name = new string('a', LENGTH);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Email_WhenNotEmpty_Passes()
    {
        // Arrange
        const int MAX_LENGTH = 255;
        Command.Email = new string('a', MAX_LENGTH);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void Email_WhenEmpty_Fails()
    {
        // Arrange
        Command.Email = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Email_WhenNull_Fails()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.Email = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void Email_WhenTooLong_Fails()
    {
        // Arrange
        const int MAX_LENGTH = 255;
        Command.Email = new string('a', MAX_LENGTH + 1);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void PhoneNumber_WhenNotEmpty_Passes()
    {
        // Arrange
        const int MAX_LENGTH = 15;
        Command.PhoneNumber = new string('a', MAX_LENGTH);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void PhoneNumber_WhenEmpty_Passes()
    {
        // Arrange
        Command.PhoneNumber = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void PhoneNumber_WhenNull_Passes()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.PhoneNumber = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void PhoneNumber_WhenTooLong_Fails()
    {
        // Arrange
        const int MAX_LENGTH = 15;
        Command.PhoneNumber = new string('a', MAX_LENGTH + 1);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void PreferredLanguage_WhenNotEmpty_Passes()
    {
        // Arrange
        const int MAX_LENGTH = 50;
        Command.PreferredLanguage = new string('a', MAX_LENGTH);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public void PreferredLanguage_WhenEmpty_Fails()
    {
        // Arrange
        Command.PreferredLanguage = "";

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void PreferredLanguage_WhenNull_Fails()
    {
        // Arrange
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Command.PreferredLanguage = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }

    [TestMethod]
    public void PreferredLanguage_WhenTooLong_Fails()
    {
        // Arrange
        const int MAX_LENGTH = 50;
        Command.PreferredLanguage = new string('a', MAX_LENGTH + 1);

        // Act
        var result = Subject.Validate(Command);

        // Assert
        Assert.IsFalse(result.IsValid);
    }
}

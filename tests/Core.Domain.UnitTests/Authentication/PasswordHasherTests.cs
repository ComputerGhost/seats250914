using Core.Domain.Authentication;

namespace Core.Domain.UnitTests.Authentication;

[TestClass]
public class PasswordHasherTests
{
    private PasswordHasher Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Subject = new();
    }

    [TestMethod]
    public void HashPassword_WhenGivenPassword_ReturnsDifferentText()
    {
        // Arrange
        const string PASSWORD = "password";

        // Act
        var hashedPassword = Subject.HashPassword(PASSWORD);

        // Assert
        Assert.AreNotEqual(PASSWORD, hashedPassword);
    }

    [TestMethod]
    public void VerifyHash_WhenGivenInvalidPassword_ReturnsFalse()
    {
        // Arrange
        const string PASSWORD = "password";
        var hashedPassword = Subject.HashPassword(PASSWORD);

        // Act
        var result = Subject.VerifyPassword(hashedPassword, "invalid");

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void VerifyHash_WhenGivenValidPassword_ReturnsTrue()
    {
        // Arrange
        const string PASSWORD = "password";
        var hashedPassword = Subject.HashPassword(PASSWORD);

        // Act
        var result = Subject.VerifyPassword(hashedPassword, PASSWORD);

        // Assert
        Assert.IsTrue(result);
    }
}

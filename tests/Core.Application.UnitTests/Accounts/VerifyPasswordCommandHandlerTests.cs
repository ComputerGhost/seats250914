using Core.Application.Accounts;
using Core.Domain.Authentication;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class VerifyPasswordCommandHandlerTests
{
    private Mock<IAccountsDatabase> MockAccountsDatabase { get; set; } = null!;
    private VerifyPasswordCommandHandler Subject { get; set; } = null!;

    private VerifyPasswordCommand MinimalValidCommand { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockAccountsDatabase = new();
        Subject = new(MockAccountsDatabase.Object);

        MinimalValidCommand = new()
        {
            Login = "login",
            Password = "password",
        };
    }

    [TestMethod]
    public async Task Handle_WhenPasswordMatches_ReturnsSuccess()
    {
        // Arrange
        const string PASSWORD = "password";
        MockAccountsDatabase
            .Setup(m => m.FetchPasswordhash(It.IsAny<string>()))
            .ReturnsAsync(HashPassword(PASSWORD));

        // Act
        var command = MinimalValidCommand;
        command.Password = PASSWORD;
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        Assert.AreEqual(Result.Success, result);
    }

    [TestMethod]
    public async Task Handle_WhenLoginMismatch_ReturnsFailure()
    {
        // Arrange
        MockAccountsDatabase
            .Setup(m => m.FetchPasswordhash(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.AreEqual(ErrorType.Failure, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenPasswordMismatch_ReturnsFailure()
    {
        // Arrange
        const string RIGHT_PASSWORD = "password";
        const string WRONG_PASSWORD = "pjomssed";
        MockAccountsDatabase
            .Setup(m => m.FetchPasswordhash(It.IsAny<string>()))
            .ReturnsAsync(HashPassword(RIGHT_PASSWORD));

        // Act
        var command = MinimalValidCommand;
        command.Password = WRONG_PASSWORD;
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        Assert.AreEqual(ErrorType.Failure, result.FirstError.Type);
    }

    private string HashPassword(string password)
    {
        return new PasswordHasher().HashPassword(password);
    }
}

using Core.Application.Accounts;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class UpdatePasswordCommandHandlerTests
{
    private Mock<IAccountsDatabase> MockAccountsDatabase { get; set; } = null!;
    private UpdatePasswordCommandHandler Subject { get; set; } = null!;

    private UpdatePasswordCommand MinimalValidCommand { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MinimalValidCommand = new() { Password = "password", };

        MockAccountsDatabase = new();
        Subject = new(MockAccountsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenAccountExists_ReturnsSuccess()
    {
        // Arrange
        MockAccountsDatabase
            .Setup(m => m.UpdatePassword(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(Result.Success, result.Value);
    }

    [TestMethod]
    public async Task Handle_WhenAccountExists_UpdatesHashedPassword()
    {
        // Arrange
        var command = MinimalValidCommand;
        command.Login = "login";
        command.Password = "password";
        MockAccountsDatabase
            .Setup(m => m.UpdatePassword(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        await Subject.Handle(command, CancellationToken.None);

        // Assert
        MockAccountsDatabase.Verify(m => m.UpdatePassword(
            It.Is<string>(v => v == command.Login),
            It.Is<string>(v => v != command.Password)));
    }

    [TestMethod]
    public async Task Handle_WhenAccountDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        MockAccountsDatabase
            .Setup(m => m.UpdatePassword(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.NotFound().Code, result.FirstError.Code);
    }
}

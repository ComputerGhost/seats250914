using Core.Application.Accounts;
using Core.Domain.Common.Exceptions;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class CreateAccountCommandHandlerTests
{
    private Mock<IAccountsDatabase> MockAccountsDatabase { get; set; } = null!;
    private CreateAccountCommandHandler Subject { get; set; } = null!;

    private CreateAccountCommand MinimalValidCommand { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MinimalValidCommand = new() { Password = "password", };

        MockAccountsDatabase = new();
        Subject = new(MockAccountsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenAccountExists_ReturnsConflict()
    {
        // Arrange
        MockAccountsDatabase
            .Setup(m => m.CreateAccount(It.IsAny<AccountEntityModel>(), It.IsAny<string>()))
            .ThrowsAsync(new AccountAlreadyExistsException("login"));

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(ErrorType.Conflict, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenAccountDoesNotExist_ReturnsSuccess()
    {
        // Arrange

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(Result.Created, result.Value);
    }

    [TestMethod]
    public async Task Handle_WhenCreatingAccount_SavedHashedPassword()
    {
        // Arrange
        var command = MinimalValidCommand;
        command.Password = "password";

        // Act
        await Subject.Handle(command, CancellationToken.None);

        // Assert
        MockAccountsDatabase.Verify(m => m.CreateAccount(
            It.IsAny<AccountEntityModel>(),
            It.Is<string>(v => v != command.Password)));

    }
}

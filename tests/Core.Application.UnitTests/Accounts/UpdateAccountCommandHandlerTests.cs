using Core.Application.Accounts;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class UpdateAccountCommandHandlerTests
{
    private Mock<IAccountsDatabase> MockAccountsDatabase { get; set; } = null!;
    private UpdateAccountCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockAccountsDatabase = new();
        Subject = new(MockAccountsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenAccountExists_ReturnsSuccess()
    {
        // Arrange
        var command = new UpdateAccountCommand();
        MockAccountsDatabase
            .Setup(m => m.UpdateAccount(It.IsAny<AccountEntityModel>()))
            .ReturnsAsync(true);

        // Act
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(Result.Updated, result.Value);
    }

    [TestMethod]
    public async Task Handle_WhenAccountExists_UpdatesDatabase()
    {
        // Arrange
        var command = new UpdateAccountCommand();
        MockAccountsDatabase
            .Setup(m => m.UpdateAccount(It.IsAny<AccountEntityModel>()))
            .ReturnsAsync(true);

        // Act
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        MockAccountsDatabase.Verify(m => m.UpdateAccount(It.IsAny<AccountEntityModel>()));
    }

    [TestMethod]
    public async Task Handle_WhenAccountDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var command = new UpdateAccountCommand();
        MockAccountsDatabase
            .Setup(m => m.UpdateAccount(It.IsAny<AccountEntityModel>()))
            .ReturnsAsync(false);

        // Act
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.NotFound().Code, result.FirstError.Code);
    }
}

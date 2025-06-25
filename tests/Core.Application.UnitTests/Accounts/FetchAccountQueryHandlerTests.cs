using Core.Application.Accounts;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class FetchAccountQueryHandlerTests
{
    private Mock<IAccountsDatabase> MockAccountsDatabase { get; set; } = null!;
    private FetchAccountQueryHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockAccountsDatabase = new();
        Subject = new(MockAccountsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenAccountExists_ReturnsAccountData()
    {
        // Arrange
        MockAccountsDatabase
            .Setup(m => m.FetchAccount(It.IsAny<string>()))
            .ReturnsAsync(new AccountEntityModel { Login = "login" });

        // Act
        var query = new FetchAccountQuery("login");
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.AreEqual("login", result.Value.Login);
    }

    [TestMethod]
    public async Task Handle_WhenAccountDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        MockAccountsDatabase
            .Setup(m => m.FetchAccount(It.IsAny<string>()))
            .ReturnsAsync((AccountEntityModel?)null);

        // Act
        var query = new FetchAccountQuery("login");
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.NotFound().Code, result.FirstError.Code);
    }
}

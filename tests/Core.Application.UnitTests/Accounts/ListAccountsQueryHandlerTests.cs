using Core.Application.Accounts;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Moq;

namespace Core.Application.UnitTests.Accounts;

[TestClass]
public class ListAccountsQueryHandlerTests
{
    private Mock<IAccountsDatabase> MockAccountsDatabase { get; set; } = null!;
    private ListAccountsQueryHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockAccountsDatabase = new();
        Subject = new(MockAccountsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenNoAccounts_ReturnsEmptySet()
    {
        // Arrange
        var query = new ListAccountsQuery();
        MockAccountsDatabase
            .Setup(m => m.ListAccounts())
            .ReturnsAsync(Array.Empty<AccountEntityModel>());

        // Act
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handle_WhenAccountsExist_ReturnsAccounts()
    {
        // Arrange
        var query = new ListAccountsQuery();
        MockAccountsDatabase
            .Setup(m => m.ListAccounts())
            .ReturnsAsync([new AccountEntityModel { Login = "login" }]);

        // Act
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, result.Data.Count());
        Assert.AreEqual("login", result.Data.First().Login);
    }
}

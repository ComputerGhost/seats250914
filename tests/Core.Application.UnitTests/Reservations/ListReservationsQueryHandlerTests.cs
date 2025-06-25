using Core.Application.Reservations;
using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class ListReservationsQueryHandlerTests
{
    private Mock<IReservationsDatabase> MockReservationsDatabase { get; set; } = null!;
    private ListReservationsQueryHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockReservationsDatabase = new();
        Subject = new(MockReservationsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenNoReservations_ReturnsEmptySet()
    {
        // Arrange
        var query = new ListReservationsQuery();
        MockReservationsDatabase
            .Setup(m => m.ListReservations())
            .ReturnsAsync([]);

        // Act
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(0, result.Data.Count());
    }

    [TestMethod]
    public async Task Handle_WhenAccountsExist_ReturnsAccounts()
    {
        // Arrange
        var query = new ListReservationsQuery();
        MockReservationsDatabase
            .Setup(m => m.ListReservations())
            .ReturnsAsync([new ReservationEntityModel
            {
                Id = 1,
                Status = ReservationStatus.AwaitingPayment.ToString(),
            }]);

        // Act
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count());
        Assert.AreEqual(1, result.Data.First().ReservationId);
    }
}

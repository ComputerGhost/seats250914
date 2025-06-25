using Core.Application.Reservations;
using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class FetchReservationQueryHandlerTests
{
    private Mock<IReservationsDatabase> MockReservationsDatabase { get; set; } = null!;
    private FetchReservationQueryHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockReservationsDatabase = new();
        Subject = new(MockReservationsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenReservationExists_ReturnsReservationData()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel
            {
                SeatNumber = 1,
                Status = ReservationStatus.AwaitingPayment.ToString(),
            });

        // Act
        var query = new FetchReservationQuery(1);
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(1, result.Value.SeatNumber);
    }

    [TestMethod]
    public async Task Handle_WhenReservationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync((ReservationEntityModel?)null);

        // Act
        var query = new FetchReservationQuery(1);
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.NotFound().Type, result.FirstError.Type);
    }
}

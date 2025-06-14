using Core.Application.Common.Enumerations;
using Core.Application.Seats;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Moq;

namespace Core.Application.UnitTests.Seats;

[TestClass]
public class ListSeatsQueryHandlerTests
{
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;
    private ListSeatsQueryHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockSeatsDatabase = new();
        Subject = new(MockSeatsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenNoSeats_ReturnsEmptySet()
    {
        // Arrange
        var query = new ListSeatsQuery();
        MockSeatsDatabase
            .Setup(m => m.ListSeats())
            .ReturnsAsync(Array.Empty<SeatEntityModel>());

        // Act
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.AreEqual(0, result.Data.Count());
    }

    [DataTestMethod]
    [DataRow(SeatStatus.Available, "Available")]
    [DataRow(SeatStatus.Locked, "Locked")]
    [DataRow(SeatStatus.AwaitingPayment, "AwaitingPayment")]
    [DataRow(SeatStatus.ReservationConfirmed, "ReservationConfirmed")]
    public async Task Handle_WhenSeatStatus_ReturnsSeatWithStatus(SeatStatus expectedStatus, string databaseStatus)
    {
        // Arrange
        var query = new ListSeatsQuery();
        MockSeatsDatabase
            .Setup(m => m.ListSeats())
            .ReturnsAsync([new SeatEntityModel { Status = databaseStatus }]);

        // Act
        var result = await Subject.Handle(query, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, result.Data.Count());
        Assert.AreEqual(expectedStatus, result.Data.First().Status);
    }
}

using Azure.Core;
using Core.Application.Common.Enumerations;
using Core.Application.Reservations;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class ApproveReservationCommandHandlerTests
{
    private Mock<IReservationsDatabase> MockReservationsDatabase { get; set; } = null!;
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;
    private ApproveReservationCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockReservationsDatabase = new();
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel());

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        Subject = new(MockReservationsDatabase.Object, MockSeatsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenReservationExists_ReturnsSuccess()
    {
        // Arrange
        var request = new ApproveReservationCommand();

        // Act
        var result = await Subject.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
    }

    [TestMethod]
    public async Task Handle_WhenReservationExists_UpdatesStatusToReservationConfirmed()
    {
        // Arrange
        const int RESERVATION_ID = 1;
        const int SEAT_NUMBER = 2;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel { SeatNumber = SEAT_NUMBER });
        var request = new ApproveReservationCommand { ReservationId = RESERVATION_ID };

        // Act
        var result = await Subject.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        MockReservationsDatabase.Verify(m => m.UpdateReservationStatus(
            It.Is<int>(p => p == RESERVATION_ID),
            It.Is<string>(p => p == ReservationStatus.ReservationConfirmed.ToString())));
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<string>(p => p == SeatStatus.ReservationConfirmed.ToString())));
    }

    [TestMethod]
    public async Task Handle_WhenReservationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync((ReservationEntityModel?)null);
        var request = new ApproveReservationCommand();

        // Act
        var result = await Subject.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
    }
}

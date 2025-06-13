using Core.Application.Accounts;
using Core.Application.Common.Enumerations;
using Core.Application.Reservations;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class RejectReservationCommandHandlerTests
{
    private Mock<IReservationsDatabase> MockReservationsDatabase { get; set; } = null!;
    private Mock<ISeatLocksDatabase> MockSeatLocksDatabase { get; set; } = null!;
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;
    private RejectReservationCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockReservationsDatabase = new();
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel());

        MockSeatLocksDatabase = new();

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        Subject = new(MockReservationsDatabase.Object, MockSeatLocksDatabase.Object, MockSeatsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenReservationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync((ReservationEntityModel?)null);

        // Act
        var request = new RejectReservationCommand();
        var result = await Subject.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.NotFound().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenReservationExists_UpdatesReservationStatus()
    {
        // Arrange
        const int RESERVATION_ID = 1;
        var request = new RejectReservationCommand { ReservationId = RESERVATION_ID };

        // Act
        var result = await Subject.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        MockReservationsDatabase.Verify(m => m.UpdateReservationStatus(
            It.Is<int>(p => p == RESERVATION_ID),
            It.Is<string>(p => p == ReservationStatus.ReservationRejected.ToString())));
    }

    [TestMethod]
    public async Task Handle_WhenReservationExists_UpdateSeatStatus()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel { SeatNumber = SEAT_NUMBER });

        // Act
        var request = new RejectReservationCommand();
        var result = await Subject.Handle(request, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<string>(p => p == SeatStatus.Available.ToString())));
    }

    [TestMethod]
    public async Task Handle_WhenReservationExists_DeletesLockLast()
    {
        // Arrange
        bool iReservationUpdated = false;
        bool isSeatUpdated = false;
        MockReservationsDatabase
            .Setup(m => m.UpdateReservationStatus(It.IsAny<int>(), It.IsAny<string>()))
            .Callback(() => iReservationUpdated = true);
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()))
            .Callback(() => isSeatUpdated = true)
            .ReturnsAsync(true);

        // Act
        var request = new RejectReservationCommand();
        var action = async () => await Subject.Handle(request, CancellationToken.None);

        // Assert
        MockSeatLocksDatabase
            .Setup(m => m.DeleteLock(It.IsAny<int>()))
            .Callback(() =>
            {
                Assert.IsTrue(iReservationUpdated);
                Assert.IsTrue(isSeatUpdated);
            });
        await action();
        MockSeatLocksDatabase.Verify(
            m => m.DeleteLock(It.IsAny<int>()),
            Times.Once());
    }
}

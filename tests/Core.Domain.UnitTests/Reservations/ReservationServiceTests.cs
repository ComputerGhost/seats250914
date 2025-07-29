using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.Reservations;
using MediatR;
using Moq;

namespace Core.Domain.UnitTests.Reservations;

[TestClass]
public class ReservationServiceTests
{
    private Mock<IMediator> MockMediator { get; set; } = null!;
    private Mock<IReservationsDatabase> MockReservationsDatabase { get; set; } = null!;
    private Mock<ISeatLocksDatabase> MockSeatLocksDatabase { get; set; } = null!;
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;
    private Mock<IUnitOfWork> MockUnitOfWork { get; set; } = null!;
    private ReservationService Subject { get; set; } = null!;

    private IdentityModel MinimalIdentity { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockMediator = new();

        MockReservationsDatabase = new();
        MockReservationsDatabase
            .Setup(m => m.CreateReservation(It.IsAny<ReservationEntityModel>()))
            .ReturnsAsync(1);
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel());
        MockReservationsDatabase
            .Setup(m => m.UpdateReservationStatus(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        MockSeatLocksDatabase = new();
        MockSeatLocksDatabase
            .Setup(m => m.AttachLocksToReservation(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
            .ReturnsAsync((IEnumerable<int> numbers, int _) => numbers.Count());
        MockSeatLocksDatabase
            .Setup(m => m.ClearLockExpirations(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync((IEnumerable<int> numbers) => numbers.Count());

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatuses(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()))
            .ReturnsAsync((IEnumerable<int> seatNumbers, string status) => seatNumbers.Count());
        MockSeatsDatabase
            .Setup(m => m.AttachSeatsToReservation(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
            .ReturnsAsync((IEnumerable<int> numbers, int _) => numbers.Count());

        MockUnitOfWork = new();
        MockUnitOfWork.Setup(m => m.Reservations).Returns(() => MockReservationsDatabase.Object);
        MockUnitOfWork.Setup(m => m.SeatLocks).Returns(() => MockSeatLocksDatabase.Object);
        MockUnitOfWork.Setup(m => m.Seats).Returns(() => MockSeatsDatabase.Object);
        MockUnitOfWork
            .Setup(m => m.RunInTransaction(It.IsAny<Func<Task<int?>>>()))
            .Returns((Func<Task<int?>> operation) => operation());
        MockUnitOfWork
            .Setup(m => m.RunInTransaction(It.IsAny<Func<Task<bool>>>()))
            .Returns((Func<Task<bool>> operation) => operation());

        Subject = new(MockMediator.Object, MockUnitOfWork.Object);

        MinimalIdentity = new()
        {
            Email = "email",
            IpAddress = "ip",
            IsStaff = true,
            Name = "name",
            PreferredLanguage = "lang",
        };
    }

    [TestMethod]
    public async Task ApproveReservation_WhenReservationExists_ReturnsTrue()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel { SeatNumbers = [1] });

        // Act
        var result = await Subject.ApproveReservation(1);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ApproveReservation_WhenReservationExists_UpdatesStatusToReservationConfirmed()
    {
        // Arrange
        const int RESERVATION_ID = 1;
        const int SEAT_NUMBER = 2;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel
            {
                SeatNumbers = [SEAT_NUMBER],
            });

        // Act
        var result = await Subject.ApproveReservation(RESERVATION_ID);

        // Assert
        Assert.IsTrue(result);
        MockReservationsDatabase.Verify(m => m.UpdateReservationStatus(
            It.Is<int>(p => p == RESERVATION_ID),
            It.Is<string>(p => p == ReservationStatus.ReservationConfirmed.ToString())));
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatuses(
            It.Is<IEnumerable<int>>(p => p.Contains(SEAT_NUMBER)),
            It.Is<string>(p => p == SeatStatus.ReservationConfirmed.ToString())));
        MockMediator.Verify(m => m.Publish(
            It.IsAny<SeatStatusesChangedNotification>(),
            It.IsAny<CancellationToken>()));
    }

    [TestMethod]
    public async Task ApproveReservation_WhenDoesNotExist_ReturnsFalse()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync((ReservationEntityModel?)null);

        // Act
        var result = await Subject.ApproveReservation(SEAT_NUMBER);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task RejectReservation_WhenDoesNotExist_ReturnsFalse()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync((ReservationEntityModel?)null);

        // Act
        var result = await Subject.RejectReservation(1);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task RejectReservation_WhenReservationExists_UpdatesReservationStatus()
    {
        // Arrange
        const int RESERVATION_ID = 1;

        // Act
        var result = await Subject.RejectReservation(RESERVATION_ID);

        // Assert
        Assert.IsTrue(result);
        MockReservationsDatabase.Verify(m => m.UpdateReservationStatus(
            It.Is<int>(p => p == RESERVATION_ID),
            It.Is<string>(p => p == ReservationStatus.ReservationRejected.ToString())));
    }

    [TestMethod]
    public async Task RejectReservation_WhenReservationExists_UpdatesSeatStatus()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel
            {
                SeatNumbers = [SEAT_NUMBER],
            });

        // Act
        var result = await Subject.RejectReservation(1);

        // Assert
        Assert.IsTrue(result);
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatuses(
            It.Is<IEnumerable<int>>(p => p.Contains(SEAT_NUMBER)),
            It.Is<string>(p => p == SeatStatus.Available.ToString())));
        MockMediator.Verify(m => m.Publish(
            It.IsAny<SeatStatusesChangedNotification>(),
            It.IsAny<CancellationToken>()));
    }

    [TestMethod]
    public async Task RejectReservation_WhenReservationExists_DeletesLocks()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel
            {
                SeatNumbers = [SEAT_NUMBER],
            });

        // Act
        var result = await Subject.RejectReservation(1);

        // Assert
        Assert.IsTrue(result);
        MockSeatLocksDatabase.Verify(m => m.DeleteLocks(
            It.Is<IEnumerable<int>>(p => p.Contains(SEAT_NUMBER))));
    }

    [TestMethod]
    public async Task ReserveSeats_WhenSuccessful_ClearsLockExpiration()
    {
        // Arrange
        const int SEAT_NUMBER = 1;

        // Act
        var result = await Subject.ReserveSeats([SEAT_NUMBER], MinimalIdentity);

        // Assert
        Assert.IsNotNull(result);
        MockReservationsDatabase.Verify(m => m.CreateReservation(It.IsAny<ReservationEntityModel>()));
        MockSeatLocksDatabase.Verify(m => m.AttachLocksToReservation(
            It.Is<IEnumerable<int>>(p => p.Contains(SEAT_NUMBER)),
            It.IsAny<int>()));
    }

    [TestMethod]
    public async Task ReserveSeats_WhenSuccessful_CreatesReservation()
    {
        // Arrange
        const int SEAT_NUMBER = 1;

        // Act
        var result = await Subject.ReserveSeats([SEAT_NUMBER], MinimalIdentity);

        // Assert
        Assert.IsNotNull(result);
        MockReservationsDatabase.Verify(m => m.CreateReservation(It.IsAny<ReservationEntityModel>()));
        MockSeatLocksDatabase.Verify(m => m.AttachLocksToReservation(
            It.Is<IEnumerable<int>>(p => p.Contains(SEAT_NUMBER)),
            It.IsAny<int>()));
    }

    [TestMethod]
    public async Task ReserveSeats_WhenSuccessful_UpdatesSeatStatus()
    {
        // Arrange
        int[] seatNumbers = [1, 2];

        // Act
        var result = await Subject.ReserveSeats(seatNumbers, MinimalIdentity);

        // Assert
        Assert.IsNotNull(result);
        foreach (var seatNumber in seatNumbers)
        {
            MockSeatsDatabase.Verify(m => m.UpdateSeatStatuses(
                It.Is<IEnumerable<int>>(p => p.Contains(seatNumber)),
                It.Is<string>(p => p == SeatStatus.AwaitingPayment.ToString())));
        }
        MockMediator.Verify(
            m => m.Publish(It.IsAny<SeatStatusesChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(1));
    }

    [TestMethod]
    public async Task ReserveSeats_WhenNoSeats_ReturnsNull()
    {
        // Act
        var result = await Subject.ReserveSeats([], MinimalIdentity);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ReserveSeats_WhenLockDeletedBeforeExpirationCleared_ReturnsNullAndRollsBackChanges()
    {
        // Arrange
        int[] seatNumbers = [1, 2];
        MockSeatLocksDatabase
            .Setup(m => m.ClearLockExpirations(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(seatNumbers.Length - 1);

        // Act
        var result = await Subject.ReserveSeats(seatNumbers, MinimalIdentity);

        // Assert
        Assert.IsNull(result);
        MockUnitOfWork.Verify(m => m.Rollback());
    }

    [TestMethod]
    public async Task ReserveSeats_WhenDatabaseConflict_ReturnsNullAndDoesNotClearLockExpirationsOrUpdateSeatStatus()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.CreateReservation(It.IsAny<ReservationEntityModel>()))
            .ReturnsAsync((int?)null);

        // Act
        var result = await Subject.ReserveSeats([1], MinimalIdentity);

        // Assert
        Assert.IsNull(result);
        MockSeatLocksDatabase.Verify(
            m => m.ClearLockExpirations(It.IsAny<IEnumerable<int>>()),
            Times.Never);
        MockSeatsDatabase.Verify(
            m => m.UpdateSeatStatuses(It.IsAny<IEnumerable<int>>(), It.IsAny<string>()),
            Times.Never);
        MockMediator.Verify(
            m => m.Publish(It.IsAny<SeatStatusesChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ReserveSeats_WhenSuccessful_ClearsLockExpirations()
    {
        // Arrange
        int[] seatNumbers = [1, 2];

        // Act
        var result = await Subject.ReserveSeats(seatNumbers, MinimalIdentity);

        // Assert
        Assert.IsNotNull(result);
        MockSeatLocksDatabase.Verify(m => m.ClearLockExpirations(It.IsAny<IEnumerable<int>>()));
    }
}

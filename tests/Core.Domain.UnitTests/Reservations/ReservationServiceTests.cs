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
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync(new SeatLockEntityModel());
        MockSeatLocksDatabase
            .Setup(m => m.ClearLockExpirations(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync((IEnumerable<int> numbers) => numbers.Count());
        MockSeatLocksDatabase
            .Setup(m => m.AttachLocksToReservation(It.IsAny<IEnumerable<int>>(), It.IsAny<int>()))
            .ReturnsAsync((IEnumerable<int> numbers, int _) => numbers.Count());

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        Subject = new(
            MockMediator.Object,
            MockReservationsDatabase.Object, 
            MockSeatLocksDatabase.Object, 
            MockSeatsDatabase.Object);

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
                SeatNumber = SEAT_NUMBER,
            });

        // Act
        var result = await Subject.ApproveReservation(RESERVATION_ID);

        // Assert
        Assert.IsTrue(result);
        MockReservationsDatabase.Verify(m => m.UpdateReservationStatus(
            It.Is<int>(p => p == RESERVATION_ID),
            It.Is<string>(p => p == ReservationStatus.ReservationConfirmed.ToString())));
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<string>(p => p == SeatStatus.ReservationConfirmed.ToString())));
        MockMediator.Verify(m => m.Publish(
            It.IsAny<SeatStatusChangedNotification>(),
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
            .ReturnsAsync(new ReservationEntityModel { SeatNumber = SEAT_NUMBER });

        // Act
        var result = await Subject.RejectReservation(1);

        // Assert
        Assert.IsTrue(result);
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<string>(p => p == SeatStatus.Available.ToString())));
        MockMediator.Verify(m => m.Publish(
            It.IsAny<SeatStatusChangedNotification>(),
            It.IsAny<CancellationToken>()));
    }

    [TestMethod]
    public async Task RejectReservation_WhenReservationExists_DeletesLockLast()
    {
        bool iReservationUpdated = false;
        bool isSeatUpdated = false;
        MockReservationsDatabase
            .Setup(m => m.UpdateReservationStatus(It.IsAny<int>(), It.IsAny<string>()))
            .Callback(() => iReservationUpdated = true)
            .ReturnsAsync(true);
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()))
            .Callback(() => isSeatUpdated = true)
            .ReturnsAsync(true);

        // Act
        async Task<bool> action() => await Subject.RejectReservation(1);

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

    [TestMethod]
    public async Task ReserveSeat_WhenSuccessful_ClearsLockExpiration()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel
            {
                SeatNumber = SEAT_NUMBER,
            });

        // Act
        var result = await Subject.ReserveSeat(SEAT_NUMBER, MinimalIdentity);

        // Assert
        Assert.IsNotNull(result);
        MockSeatLocksDatabase.Verify(m => m.ClearLockExpiration(
            It.Is<int>(p => p == SEAT_NUMBER)));
    }

    [TestMethod]
    public async Task ReserveSeat_WhenSuccessful_CreatesReservation()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel()
            {
                SeatNumber = SEAT_NUMBER,
            });

        // Act
        var result = await Subject.ReserveSeat(SEAT_NUMBER, MinimalIdentity);

        // Assert
        Assert.IsNotNull(result);
        MockReservationsDatabase.Verify(m => m.CreateReservation(
            It.Is<ReservationEntityModel>(p => p.SeatNumber == SEAT_NUMBER)));
    }

    [TestMethod]
    public async Task ReserveSeat_WhenSuccessful_UpdatesSeatStatus()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockReservationsDatabase
            .Setup(m => m.FetchReservation(It.IsAny<int>()))
            .ReturnsAsync(new ReservationEntityModel
            {
                SeatNumber = SEAT_NUMBER,
            });

        // Act
        var result = await Subject.ReserveSeat(SEAT_NUMBER, MinimalIdentity);

        // Assert
        Assert.IsNotNull(result);
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<string>(p => p == SeatStatus.AwaitingPayment.ToString())));
        MockMediator.Verify(m => m.Publish(
            It.IsAny<SeatStatusChangedNotification>(),
            It.IsAny<CancellationToken>()));
    }

    [TestMethod]
    public async Task ReserveSeat_WhenLockDeletedBeforeExpirationCleared_ReturnsNull()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync((SeatLockEntityModel?)null);

        // Act
        var result = await Subject.ReserveSeat(1, MinimalIdentity);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ReserveSeat_WhenLockDeletedBeforeExpirationCleared_DoesNotCreateReservation()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync((SeatLockEntityModel?)null);

        // Act
        var result = await Subject.ReserveSeat(1, MinimalIdentity);

        // Assert
        MockReservationsDatabase.Verify(
            m => m.CreateReservation(It.IsAny<ReservationEntityModel>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ReserveSeat_WhenLockDeletedBeforeExpirationCleared_DoesNotUpdateSeatStatus()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync((SeatLockEntityModel?)null);

        // Act
        var result = await Subject.ReserveSeat(1, MinimalIdentity);

        // Assert
        MockSeatsDatabase.Verify(
            m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
        MockMediator.Verify(
            m => m.Publish(It.IsAny<SeatStatusChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ReserveSeat_WhenDatabaseConflict_DoesNotUpdateSeatStatus()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.CreateReservation(It.IsAny<ReservationEntityModel>()))
            .ReturnsAsync((int?)null);

        // Act
        var result = await Subject.ReserveSeat(1, MinimalIdentity);

        // Assert
        MockSeatsDatabase.Verify(
            m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
        MockMediator.Verify(
            m => m.Publish(It.IsAny<SeatStatusChangedNotification>(), It.IsAny<CancellationToken>()),
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
            MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
                It.Is<int>(p => p == seatNumber),
                It.Is<string>(p => p == SeatStatus.AwaitingPayment.ToString())));
        }
        MockMediator.Verify(
            m => m.Publish(It.IsAny<SeatStatusChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Exactly(seatNumbers.Length));
    }

    [TestMethod]
    public async Task ReserveSeats_WhenLockDeletedBeforeExpirationCleared_ReturnsNull()
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
    }

    [TestMethod]
    public async Task ReserveSeats_WhenLockDeletedBeforeExpirationCleared_DoesNotCreateReservation()
    {
        // Arrange
        int[] seatNumbers = [1, 2];
        MockSeatLocksDatabase
            .Setup(m => m.ClearLockExpirations(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(seatNumbers.Length - 1);

        // Act
        var result = await Subject.ReserveSeats(seatNumbers, MinimalIdentity);

        // Assert
        MockReservationsDatabase.Verify(
            m => m.CreateReservation(It.IsAny<ReservationEntityModel>()),
            Times.Exactly(1)); // But it will be deleted.
        MockReservationsDatabase.Verify(
            m => m.DeleteReservation(It.IsAny<int>()),
            Times.Exactly(1));
    }

    [TestMethod]
    public async Task ReserveSeats_WhenLockDeletedBeforeExpirationCleared_DoesNotUpdateSeatStatus()
    {
        // Arrange
        int[] seatNumbers = [1, 2];
        MockSeatLocksDatabase
            .Setup(m => m.ClearLockExpirations(It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(seatNumbers.Length - 1);

        // Act
        var result = await Subject.ReserveSeats(seatNumbers, MinimalIdentity);

        // Assert
        MockSeatsDatabase.Verify(
            m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
        MockMediator.Verify(
            m => m.Publish(It.IsAny<SeatStatusChangedNotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

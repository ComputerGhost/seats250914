using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.Reservations;
using MediatR;
using Moq;

namespace Core.Domain.UnitTests.Reservations;

[TestClass]
public class SeatLockServiceTests
{
    private ConfigurationEntityModel Configuration { get; set; } = null!;
    private Mock<IMediator> MockMediator { get; set; } = null!;
    private Mock<ISeatLocksDatabase> MockSeatLocksDatabase { get; set; } = null!;
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;
    private SeatLockService Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Configuration = ConfigurationEntityModel.Default;

        var mockConfigurationDatabase = new Mock<IConfigurationDatabase>();
        mockConfigurationDatabase
            .Setup(m => m.FetchConfiguration())
            .ReturnsAsync(() => Configuration);

        MockMediator = new Mock<IMediator>();

        MockSeatLocksDatabase = new();
        MockSeatLocksDatabase
            .Setup(m => m.FetchExpiredLocks(It.IsAny<DateTimeOffset>()))
            .ReturnsAsync([]);

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        Subject = new(
            MockMediator.Object,
            mockConfigurationDatabase.Object,
            MockSeatLocksDatabase.Object,
            MockSeatsDatabase.Object);
    }

    [TestMethod]
    public async Task ClearExpiredLocks_ForExpiringLocks_ConsidersGracePeriod()
    {
        // Arrange
        Configuration.GracePeriodSeconds = 60;

        // Act
        await Subject.ClearExpiredLocks();

        // Assert
        var actualCutoff = (DateTimeOffset)MockSeatLocksDatabase.Invocations[0].Arguments[0];
        var actualGracePeriod = (actualCutoff - DateTimeOffset.UtcNow).TotalSeconds;
        Assert.AreEqual(Configuration.GracePeriodSeconds, actualGracePeriod, 10);
    }

    [TestMethod]
    public async Task ClearExpiredLocks_ForExpiringLocks_ClearsExpiredLocks()
    {
        // Arrange
        var expiredLock = new SeatLockEntityModel { SeatNumber = 1, };
        MockSeatLocksDatabase
            .Setup(m => m.FetchExpiredLocks(It.IsAny<DateTimeOffset>()))
            .ReturnsAsync([expiredLock]);

        // Act
        await Subject.ClearExpiredLocks();

        // Assert
        MockSeatLocksDatabase.Verify(m => m.DeleteLock(
            It.Is<int>(p => p == expiredLock.SeatNumber)));
    }

    [TestMethod]
    public async Task ClearExpiredLocks_ForExpiringLocks_ResetsSeatStatuses()
    {
        // Arrange
        var expiredLock = new SeatLockEntityModel { SeatNumber = 1, };
        MockSeatLocksDatabase
            .Setup(m => m.FetchExpiredLocks(It.IsAny<DateTimeOffset>()))
            .ReturnsAsync([expiredLock]);

        // Act
        await Subject.ClearExpiredLocks();

        // Assert
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == expiredLock.SeatNumber),
            It.Is<string>(p => p == SeatStatus.Available.ToString())));
        MockMediator.Verify(m => m.Publish(
            It.IsAny<SeatStatusChangedNotification>(),
            It.IsAny<CancellationToken>()));
    }

    [TestMethod]
    public async Task LockSeat_WhenSuccessful_LocksSeatWithExpiration()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        const int EXPIRATION_SECONDS = 10;
        Configuration.MaxSecondsToConfirmSeat = EXPIRATION_SECONDS;

        // Act
        var startTime = DateTime.UtcNow;
        await Subject.LockSeat(SEAT_NUMBER, "");
        var endTime = DateTime.UtcNow;

        // Assert
        var minExpectedExpiration = startTime.AddSeconds(EXPIRATION_SECONDS);
        var maxExpectedExpiration = endTime.AddSeconds(EXPIRATION_SECONDS);
        MockSeatLocksDatabase.Verify(m => m.LockSeat(It.Is<SeatLockEntityModel>(p =>
            p.SeatNumber == SEAT_NUMBER &&
            minExpectedExpiration < p.Expiration && p.Expiration < maxExpectedExpiration
        )));
    }

    [TestMethod]
    public async Task LockSeat_WhenSuccessful_UpdatesSeatStatus()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockSeatLocksDatabase
            .Setup(m => m.LockSeat(It.IsAny<SeatLockEntityModel>()))
            .ReturnsAsync(true);

        // Act
        await Subject.LockSeat(SEAT_NUMBER, "");

        // Assert
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<string>(p => p == SeatStatus.Locked.ToString())));
        MockMediator.Verify(m => m.Publish(
            It.IsAny<SeatStatusChangedNotification>(),
            It.IsAny<CancellationToken>()));
    }

    [TestMethod]
    public async Task LockSeat_WhenSuccessful_ReturnsSeatLocks()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        MockSeatLocksDatabase
            .Setup(m => m.LockSeat(It.IsAny<SeatLockEntityModel>()))
            .ReturnsAsync(true);

        // Act
        var result = await Subject.LockSeat(SEAT_NUMBER, "");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(SEAT_NUMBER, result.SeatNumber);
        Assert.AreNotEqual(0, result.Key.Length);
    }

    [TestMethod]
    public async Task LockSeat_WhenSuccessful_SetsExpirationWithoutGracePeriod()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.LockSeat(It.IsAny<SeatLockEntityModel>()))
            .ReturnsAsync(true);
        Configuration.MaxSecondsToConfirmSeat = 60;
        Configuration.GracePeriodSeconds = 60;

        // Act
        var result = await Subject.LockSeat(1, "");

        // Assert
        Assert.IsNotNull(result);
        var expiresInSeconds = (result.Expiration - DateTimeOffset.UtcNow).TotalSeconds;
        Assert.AreEqual(Configuration.MaxSecondsToConfirmSeat, expiresInSeconds, 10);
    }

    [TestMethod]
    public async Task LockSeat_WhenSeatIsUnavailable_ReturnsNull()
    {
        // Arrange
        const int SEAT_NUMBER = 1;

        // Act
        var result = await Subject.LockSeat(SEAT_NUMBER, "");

        // Assert
        Assert.IsNull(result);
    }
}

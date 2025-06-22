using Core.Application.Reservations;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class ClearExpiredLocksCommandHandlerTests
{
    private ConfigurationEntityModel Configuration { get; set; } = null!;
    private Mock<ISeatLocksDatabase> MockSeatLocksDatabase { get; set; } = null!;
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;
    private ClearExpiredLocksCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Configuration = ConfigurationEntityModel.Default;

        var mockConfigurationDatabase = new Mock<IConfigurationDatabase>();
        mockConfigurationDatabase
            .Setup(m => m.FetchConfiguration())
            .ReturnsAsync(() => Configuration);

        MockSeatLocksDatabase = new();
        MockSeatLocksDatabase
            .Setup(m => m.LockSeat(It.IsAny<SeatLockEntityModel>()))
            .ReturnsAsync(true);

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.FetchSeat(It.IsAny<int>()))
            .ReturnsAsync(new SeatEntityModel());

        Subject = new(
            mockConfigurationDatabase.Object, 
            MockSeatLocksDatabase.Object, 
            MockSeatsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_ForExpiringLocks_ConsidersGracePeriod()
    {
        // Arrange
        Configuration.GracePeriodSeconds = 60;

        // Act
        await Subject.Handle(new ClearExpiredLocksCommand(), CancellationToken.None);

        // Assert
        var actualCutoff = (DateTimeOffset)MockSeatLocksDatabase.Invocations[0].Arguments[0];
        var actualGracePeriod = (actualCutoff - DateTimeOffset.UtcNow).TotalSeconds;
        Assert.AreEqual(Configuration.GracePeriodSeconds, actualGracePeriod, 10);
    }

    [TestMethod]
    public async Task Handle_ForExpiringLocks_ClearsExpiredLocks()
    {
        // Arrange

        // Act
        await Subject.Handle(new ClearExpiredLocksCommand(), CancellationToken.None);

        // Assert
        MockSeatLocksDatabase.Verify(m => m.ClearExpiredLocks(It.IsAny<DateTimeOffset>()));
    }

    [TestMethod]
    public async Task Handle_ForExpiringLocks_ResetsSeatStatuses()
    {
        // Arrange

        // Act
        await Subject.Handle(new ClearExpiredLocksCommand(), CancellationToken.None);

        // Assert
        MockSeatsDatabase.Verify(m => m.ResetUnlockedSeatStatuses());
    }
}

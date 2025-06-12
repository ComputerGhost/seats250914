using Core.Application.Reservations;
using Core.Application.Seats.Enumerations;
using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class LockSeatCommandHandlerTests
{
    private Mock<IConfigurationDatabase> MockConfigurationDatabase { get; set; } = null!;
    private Mock<IReservationAuthorizationChecker> MockReservationAuthorizationChecker { get; set; } = null!;
    private Mock<ISeatLocksDatabase> MockSeatLocksDatabase { get; set; } = null!;
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;
    private LockSeatCommandHandler Subject { get; set; } = null!;

    private LockSeatCommand MinimalValidCommand { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockConfigurationDatabase = new();
        MockConfigurationDatabase
            .Setup(m => m.FetchConfiguration())
            .ReturnsAsync(new ConfigurationEntityModel());

        MockReservationAuthorizationChecker = new();
        MockReservationAuthorizationChecker
            .Setup(m => m.CanMakeReservation(It.IsAny<ConfigurationEntityModel>()))
            .Returns(true);

        MockSeatLocksDatabase = new();
        MockSeatLocksDatabase
            .Setup(m => m.LockSeat(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.FetchSeat(It.IsAny<int>()))
            .ReturnsAsync(new SeatEntityModel());
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        Subject = new(
            MockConfigurationDatabase.Object, 
            MockReservationAuthorizationChecker.Object,
            MockSeatLocksDatabase.Object, 
            MockSeatsDatabase.Object);

        MinimalValidCommand = new() { SeatNumber = 1, };
    }

    [TestMethod]
    public async Task Handle_WhenSuccessful_LocksSeat()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        const int EXPIRATION_SECONDS = 10;
        MockConfigurationDatabase
            .Setup(m => m.FetchConfiguration())
            .ReturnsAsync(new ConfigurationEntityModel { MaxSecondsToConfirmSeat = EXPIRATION_SECONDS });
        var command = MinimalValidCommand;
        command.SeatNumber = SEAT_NUMBER;

        // Act
        var startTime = DateTime.UtcNow;
        await Subject.Handle(command, CancellationToken.None);
        var endTime = DateTime.UtcNow;

        // Assert
        var minExpectedExpiration = startTime.AddSeconds(EXPIRATION_SECONDS);
        var maxExpectedExpiration = endTime.AddSeconds(EXPIRATION_SECONDS);
        MockSeatLocksDatabase.Verify(m => m.LockSeat(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<DateTimeOffset>(p => minExpectedExpiration < p && p < maxExpectedExpiration),
            It.IsAny<string>()));
    }

    [TestMethod]
    public async Task Handle_WhenSuccessful_UpdatesSeatStatus()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        var command = MinimalValidCommand;
        command.SeatNumber = SEAT_NUMBER;

        // Act
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<string>(p => p == SeatStatus.Locked.ToString())));
    }

    [TestMethod]
    public async Task Handle_WhenSuccessful_ReturnsSeatLock()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        var command = MinimalValidCommand;
        command.SeatNumber = SEAT_NUMBER;

        // Act
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(SEAT_NUMBER, result.Value.SeatNumber);
        Assert.AreNotEqual(0, result.Value.SeatKey.Length);
    }

    [TestMethod]
    public async Task Handle_WhenReservationsAreClosed_ReturnsFailure()
    {
        // Arrange
        MockReservationAuthorizationChecker
            .Setup(m => m.CanMakeReservation(It.IsAny<ConfigurationEntityModel>()))
            .Returns(false);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Failure().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenSeatIsUnavailable_ReturnsConflict()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.LockSeat(It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Conflict().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenSeatIsInvalid_ReturnsNotFound()
    {
        // Arrange
        MockSeatsDatabase
            .Setup(m => m.FetchSeat(It.IsAny<int>()))
            .ReturnsAsync((SeatEntityModel?)null);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.NotFound().Type, result.FirstError.Type);
    }
}

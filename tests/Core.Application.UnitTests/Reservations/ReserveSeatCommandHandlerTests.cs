using Core.Application.Common.Enumerations;
using Core.Application.Reservations;
using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class ReserveSeatCommandHandlerTests
{
    private Mock<IAuthorizationChecker> MockReservationAuthorizationChecker { get; set; } = null!;
    private Mock<IReservationsDatabase> MockReservationsDatabase { get; set; } = null!;
    private Mock<ISeatLocksDatabase> MockSeatLocksDatabase { get; set; } = null!;
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;
    private ReserveSeatCommandHandler Subject { get; set; } = null!;

    private ReserveSeatCommand MinimalValidCommand { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockReservationAuthorizationChecker = new();
        MockReservationAuthorizationChecker
            .Setup(m => m.GetReserveSeatAuthorization(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.Success);

        MockReservationsDatabase = new();

        MockSeatLocksDatabase = new();
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync(new SeatLockEntityModel());
        MockSeatLocksDatabase
            .Setup(m => m.LockSeat(It.IsAny<SeatLockEntityModel>()))
            .ReturnsAsync(true);

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        Subject = new(
            MockReservationAuthorizationChecker.Object,
            MockReservationsDatabase.Object,
            MockSeatLocksDatabase.Object,
            MockSeatsDatabase.Object);

        MinimalValidCommand = new()
        {
            Email = "email",
            Name = "name",
            PreferredLanguage = "lang",
            SeatNumber = 1,
            SeatKey = new string('a', 44),
        };
    }

    [TestMethod]
    public async Task Handle_WhenSuccessful_ClearsLockExpiration()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        var command = MinimalValidCommand;
        command.SeatNumber = SEAT_NUMBER;

        // Act
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        MockSeatLocksDatabase.Verify(m => m.ClearLockExpiration(
            It.Is<int>(p => p == SEAT_NUMBER)));
    }

    [TestMethod]
    public async Task Handle_WhenSuccessful_CreatesReservation()
    {
        // Arrange
        const int SEAT_NUMBER = 1;
        var command = MinimalValidCommand;
        command.SeatNumber = SEAT_NUMBER;

        // Act
        var result = await Subject.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
        MockReservationsDatabase.Verify(m => m.CreateReservation(
            It.Is<ReservationEntityModel>(p => p.SeatNumber == SEAT_NUMBER)));
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
        Assert.IsFalse(result.IsError);
        MockSeatsDatabase.Verify(m => m.UpdateSeatStatus(
            It.Is<int>(p => p == SEAT_NUMBER),
            It.Is<string>(p => p == SeatStatus.AwaitingPayment.ToString())));
    }

    [TestMethod]
    public async Task Handle_WhenLockDeletedBeforeExpirationCleared_ReturnsFailure()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync((SeatLockEntityModel?)null);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Failure().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenLockDeletedBeforeExpirationCleared_DoesNotCreateReservation()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync((SeatLockEntityModel?)null);

        // Act
        await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        MockReservationsDatabase.Verify(
            m => m.CreateReservation(It.IsAny<ReservationEntityModel>()), 
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenLockDeletedBeforeExpirationCleared_DoesNotUpdateSeatStatus()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync((SeatLockEntityModel?)null);

        // Act
        await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        MockSeatsDatabase.Verify(
            m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_ReturnsFailure()
    {
        // Arrange
        MockReservationAuthorizationChecker
            .Setup(m => m.GetReserveSeatAuthorization(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Failure().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenFailure_DoesNotClearLockExpiration()
    {
        // Arrange
        MockReservationAuthorizationChecker
            .Setup(m => m.GetReserveSeatAuthorization(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        MockSeatLocksDatabase.Verify(
            m => m.ClearLockExpiration(It.IsAny<int>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_DoesNotCreateReservation()
    {
        // Arrange
        MockReservationAuthorizationChecker
            .Setup(m => m.GetReserveSeatAuthorization(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        MockReservationsDatabase.Verify(
            m => m.CreateReservation(It.IsAny<ReservationEntityModel>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_DoesNotUpdateSeatStatus()
    {
        // Arrange
        MockReservationAuthorizationChecker
            .Setup(m => m.GetReserveSeatAuthorization(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        await Subject.Handle(MinimalValidCommand, CancellationToken.None);

        // Assert
        MockSeatsDatabase.Verify(
            m => m.UpdateSeatStatus(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }
}

using Core.Application.Reservations;
using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Reservations;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class LockSeatCommandHandlerTests
{
    private Mock<IAuthorizationChecker> MockAuthorizationChecker { get; set; } = null!;
    private Mock<ISeatLockService> MockSeatLockService { get; set; } = null!;
    private LockSeatCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockAuthorizationChecker = new();
        MockSeatLockService = new();

        Subject = new(MockAuthorizationChecker.Object, MockSeatLockService.Object);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedToLockSeat_ReturnsUnauthorized()
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetLockSeatAuthorization(It.IsAny<IdentityModel>()))
            .ReturnsAsync(AuthorizationResult.ReservationsAreClosed);

        // Act
        var result = await Subject.Handle(new LockSeatCommand(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Unauthorized().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedToLockSeat_DoesNotLockSeat()
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetLockSeatAuthorization(It.IsAny<IdentityModel>()))
            .ReturnsAsync(AuthorizationResult.ReservationsAreClosed);

        // Act
        var result = await Subject.Handle(new LockSeatCommand(), CancellationToken.None);

        // Assert
        MockSeatLockService.Verify(
            m => m.LockSeat(It.IsAny<int>(), It.IsAny<string>()),
            Times.Never);
    }

    [DataTestMethod]
    [DataRow(AuthorizationRejectionReason.KeyIsInvalid)]
    [DataRow(AuthorizationRejectionReason.KeyIsExpired)]
    [DataRow(AuthorizationRejectionReason.ReservationsAreClosed)]
    [DataRow(AuthorizationRejectionReason.SeatIsNotLocked)]
    [DataRow(AuthorizationRejectionReason.TooManyReservationsForEmail)]
    [DataRow(AuthorizationRejectionReason.TooManySeatLocksForIpAddress)]
    public async Task Handle_WhenNotAuthorizedToLockSeat_ReturnsDetailsInMetadata(AuthorizationRejectionReason rejectionReason)
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetLockSeatAuthorization(It.IsAny<IdentityModel>()))
            .ReturnsAsync(AuthorizationResult.Failure(rejectionReason));

        // Act
        var result = await Subject.Handle(new LockSeatCommand(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.IsNotNull(result.FirstError.Metadata);
        Assert.IsNotNull(result.FirstError.Metadata["details"]);
        var details = (AuthorizationResult)result.FirstError.Metadata["details"];
        Assert.AreEqual(rejectionReason, details.FailureReason);
    }
}

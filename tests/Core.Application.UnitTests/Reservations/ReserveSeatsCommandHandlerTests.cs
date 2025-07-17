using Core.Application.Reservations;
using Core.Domain.Authorization;
using Core.Domain.Common.Enumerations;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.Reservations;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class ReserveSeatsCommandHandlerTests
{
    private Mock<IAuthorizationChecker> MockAuthorizationChecker { get; set; } = null!;
    private Mock<IEmailsDatabase> MockEmailsDatabase { get; set; } = null!;
    private Mock<IReservationService> MockReservationService { get; set; } = null!;
    private ReserveSeatsCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockAuthorizationChecker = new();
        MockAuthorizationChecker
            .Setup(m => m.GetReserveSeatsAuthorization(It.IsAny<IdentityModel>(), It.IsAny<IDictionary<int, string>>()))
            .ReturnsAsync(AuthorizationResult.Success);

        MockEmailsDatabase = new();
        MockReservationService = new();

        Subject = new(MockAuthorizationChecker.Object, MockEmailsDatabase.Object, MockReservationService.Object);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_ReturnsUnauthorized()
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetReserveSeatsAuthorization(It.IsAny<IdentityModel>(), It.IsAny<IDictionary<int, string>>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(new ReserveSeatsCommand(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Unauthorized().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_DoesNotReserveSeat()
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetReserveSeatsAuthorization(It.IsAny<IdentityModel>(), It.IsAny<IDictionary<int, string>>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(new ReserveSeatsCommand(), CancellationToken.None);

        // Assert
        MockReservationService.Verify(
            m => m.ReserveSeats(It.IsAny<IList<int>>(), It.IsAny<IdentityModel>()),
            Times.Never);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_DoesNotEnqueueEmail()
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetReserveSeatsAuthorization(It.IsAny<IdentityModel>(), It.IsAny<IDictionary<int, string>>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(new ReserveSeatsCommand(), CancellationToken.None);

        // Assert
        MockEmailsDatabase.Verify(
            m => m.EnqueueEmail(It.IsAny<string>(), It.IsAny<int>()),
            Times.Never);
    }

    [DataTestMethod]
    [DataRow(AuthorizationRejectionReason.KeyIsInvalid)]
    [DataRow(AuthorizationRejectionReason.KeyIsExpired)]
    [DataRow(AuthorizationRejectionReason.ReservationsAreClosed)]
    [DataRow(AuthorizationRejectionReason.SeatIsNotLocked)]
    [DataRow(AuthorizationRejectionReason.TooManyReservationsForEmail)]
    [DataRow(AuthorizationRejectionReason.TooManySeatLocksForIpAddress)]
    public async Task Handle_WhenNotAuthorizedToReserveSeat_ReturnsDetailsInMetadata(AuthorizationRejectionReason rejectionReason)
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetReserveSeatsAuthorization(It.IsAny<IdentityModel>(), It.IsAny<IDictionary<int, string>>()))
            .ReturnsAsync(AuthorizationResult.Failure(rejectionReason));

        // Act
        var result = await Subject.Handle(new ReserveSeatsCommand(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.IsNotNull(result.FirstError.Metadata);
        Assert.IsNotNull(result.FirstError.Metadata["details"]);
        var details = (AuthorizationResult)result.FirstError.Metadata["details"];
        Assert.AreEqual(rejectionReason, details.FailureReason);
    }

    [TestMethod]
    public async Task Handle_WhenSuccess_EnqueuesEmail()
    {
        // Arrange
        const int RESERVATION_ID = 1;
        MockReservationService
            .Setup(m => m.ReserveSeat(It.IsAny<int>(), It.IsAny<IdentityModel>()))
            .ReturnsAsync(RESERVATION_ID);

        // Act
        var result = await Subject.Handle(new ReserveSeatsCommand(), CancellationToken.None);

        // Assert
        MockEmailsDatabase.Verify(
            m => m.EnqueueEmail(
                It.Is<string>(p => p == EmailType.UserSubmittedReservation.ToString()),
                It.Is<int>(p => p == RESERVATION_ID)),
            Times.Once());
    }
}

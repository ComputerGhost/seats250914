using Core.Application.Reservations;
using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Reservations;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class ReserveSeatCommandHandlerTests
{
    private Mock<IAuthorizationChecker> MockAuthorizationChecker { get; set; } = null!;
    private Mock<IReservationService> MockReservationService { get; set; } = null!;
    private ReserveSeatCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockAuthorizationChecker = new();
        MockReservationService = new();

        Subject = new(MockAuthorizationChecker.Object, MockReservationService.Object);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_ReturnsUnauthorized()
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetReserveSeatAuthorization(It.IsAny<IdentityModel>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(new ReserveSeatCommand(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Unauthorized().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_DoesNotReserveSeat()
    {
        // Arrange
        MockAuthorizationChecker
            .Setup(m => m.GetReserveSeatAuthorization(It.IsAny<IdentityModel>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(new ReserveSeatCommand(), CancellationToken.None);

        // Assert
        MockReservationService.Verify(
            m => m.ReserveSeat(It.IsAny<int>(), It.IsAny<IdentityModel>()),
            Times.Never);
    }
}

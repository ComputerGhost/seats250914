using Core.Application.Reservations;
using Core.Domain.Authorization;
using Core.Domain.Reservations;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class UnlockSeatCommandHandlerTests
{
    private Mock<IAuthorizationChecker> MockAuthorizationChecker { get; set; } = null!;
    private Mock<ISeatLockService> MockSeatLockService { get; set; } = null!;
    private UnlockSeatCommandHandler Subject { get; set; } = null!;

    private UnlockSeatCommand MinimalCommand { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockAuthorizationChecker = new();
        MockSeatLockService = new();
        Subject = new(MockAuthorizationChecker.Object, MockSeatLockService.Object);

        MinimalCommand = new UnlockSeatCommand
        {
            SeatKey = "",
            SeatNumber = 1,
        };
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_ReturnsUnauthorized()
    {
        MockAuthorizationChecker
            .Setup(m => m.GetUnlockSeatAuthorization(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(MinimalCommand, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Unauthorized().Type, result.FirstError.Type);
    }

    [TestMethod]
    public async Task Handle_WhenNotAuthorizedForLockedSeat_DoesNotUnlockSeat()
    {
        MockAuthorizationChecker
            .Setup(m => m.GetUnlockSeatAuthorization(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(AuthorizationResult.KeyIsInvalid);

        // Act
        var result = await Subject.Handle(MinimalCommand, CancellationToken.None);

        // Assert
        MockSeatLockService.Verify(
            m => m.UnlockSeat(It.IsAny<int>()),
            Times.Never);
    }
}

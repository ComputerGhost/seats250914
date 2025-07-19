using Core.Application.Reservations;
using Core.Domain.Common.Models;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Reservations;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class AdminReserveSeatCommandHandlerTests
{
    private Mock<ISeatLockService> MockSeatLockService { get; set; } = null!;
    private Mock<IReservationService> MockReservationService { get; set; } = null!;
    private AdminReserveSeatCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockSeatLockService = new();
        MockSeatLockService
            .Setup(m => m.LockSeat(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new SeatLockEntityModel());

        MockReservationService = new();
        MockReservationService
            .Setup(m => m.ReserveSeats(It.IsAny<IList<int>>(), It.IsAny<IdentityModel>()))
            .ReturnsAsync(1);

        Subject = new(MockReservationService.Object, MockSeatLockService.Object);
    }

    [TestMethod]
    public async Task Handle_WithTestDefaults_ReturnsReservationId()
    {
        // Act
        var result = await Subject.Handle(new AdminReserveSeatCommand(), CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsError);
    }

    [TestMethod]
    public async Task Handle_WhenSeatTaken_ReturnsConflict()
    {
        // Arrange
        MockSeatLockService
            .Setup(m => m.LockSeat(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync((SeatLockEntityModel?)null);

        // Act
        var result = await Subject.Handle(new AdminReserveSeatCommand(), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.Conflict().Type, result.FirstError.Type);
    }
}

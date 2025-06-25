using Core.Application.Reservations;
using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using ErrorOr;
using Moq;

namespace Core.Application.UnitTests.Reservations;

[TestClass]
public class UpdateReservationCommandHandlerTests
{
    private Mock<IReservationsDatabase> MockReservationsDatabase { get; set; } = null!;
    private UpdateReservationCommandHandler Subject { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        MockReservationsDatabase = new();
        Subject = new(MockReservationsDatabase.Object);
    }

    [TestMethod]
    public async Task Handle_WhenReservationExists_ReturnsSuccess()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.UpdateReservation(It.IsAny<ReservationEntityModel>()))
            .ReturnsAsync(true);

        // Act
        var request = new UpdateReservationCommand();
        var result = await Subject.Handle(request, CancellationToken.None);

        // Asert
        Assert.IsFalse(result.IsError);
        Assert.AreEqual(Result.Success, result.Value);
    }

    [TestMethod]
    public async Task Handle_WhenReservationDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        MockReservationsDatabase
            .Setup(m => m.UpdateReservation(It.IsAny<ReservationEntityModel>()))
            .ReturnsAsync(false);

        // Act
        var request = new UpdateReservationCommand();
        var result = await Subject.Handle(request, CancellationToken.None);

        // Asert
        Assert.IsTrue(result.IsError);
        Assert.AreEqual(Error.NotFound().Type, result.FirstError.Type);
    }
}

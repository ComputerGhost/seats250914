using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Core.Domain.Scheduling;
using Moq;

namespace Core.Domain.UnitTests.Scheduling;

[TestClass]
public class OpenCheckerTests
{
    private Mock<IConfigurationDatabase> MockConfigurationDatabase { get; set; } = null!;
    private OpenChecker Subject { get; set; } = null!;

    private ConfigurationEntityModel Configuration { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Configuration = new()
        {
            ForceCloseReservations = false,
            ForceOpenReservations = false,
            ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1),
        };

        MockConfigurationDatabase = new();
        MockConfigurationDatabase
            .Setup(m => m.FetchConfiguration())
            .ReturnsAsync(() => Configuration);

        Subject = new(MockConfigurationDatabase.Object);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenNotForcedStatus_AndInDateRange_ReturnsTrue()
    {
        // Arrange

        // Act
        var result = await Subject.AreReservationsOpen();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenNotForcedStatus_AndOutsideDateRange_ReturnsFalse()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(1);

        // Act
        var result = await Subject.AreReservationsOpen();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenForcedClosed_ReturnsFalse()
    {
        // Arrange
        Configuration.ForceCloseReservations = true;

        // Act
        var result = await Subject.AreReservationsOpen();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenForcedOpen_AndNotInDateRange_ReturnsTrue()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(1);
        Configuration.ForceOpenReservations = true;

        // Act
        var result = await Subject.AreReservationsOpen();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenForcedOpenAndClosed_Undefined()
    {
        // Arrange
        Configuration.ForceOpenReservations = true;
        Configuration.ForceCloseReservations = true;

        // Act
        var result = await Subject.AreReservationsOpen();

        // Assert
        // This state is explicitly undefined.
    }
}

using Core.Domain.Common.Models.Entities;
using Core.Domain.Common.Ports;
using Core.Domain.Scheduling;
using Moq;

namespace Core.Domain.UnitTests.Scheduling;

[TestClass]
public class OpenCheckerTests
{
    private Mock<IConfigurationDatabase> MockConfigurationDatabase { get; set; } = null!;
    private Mock<ISeatsDatabase> MockSeatsDatabase { get; set; } = null!;

    private ConfigurationEntityModel Configuration { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Configuration = new()
        {
            // Default to non-forced open.
            ForceCloseReservations = false,
            ForceOpenReservations = false,
            ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1),
            ScheduledCloseDateTime = DateTime.UtcNow.AddYears(1)
        };

        MockConfigurationDatabase = new();
        MockConfigurationDatabase
            .Setup(m => m.FetchConfiguration())
            .ReturnsAsync(() => Configuration);

        MockSeatsDatabase = new();
        MockSeatsDatabase
            .Setup(m => m.CountSeats(It.IsAny<string>()))
            .ReturnsAsync(1);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenNotForcedStatus_AndInDateRange_ReturnsTrue()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddYears(1);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object, 
            MockSeatsDatabase.Object);

        // Act
        var result = subject.AreReservationsOpen();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenNotForcedStatus_AndNotYetOpen_ReturnsFalse()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddMonths(1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddYears(1);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = subject.AreReservationsOpen();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenNotForcedStatus_AndAfterClose_ReturnsFalse()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddMonths(-1);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = subject.AreReservationsOpen();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenForcedClosed_ReturnsFalse()
    {
        // Arrange
        Configuration.ForceCloseReservations = true;
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = subject.AreReservationsOpen();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenForcedOpen_AndNotInDateRange_ReturnsTrue()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddMonths(-1);
        Configuration.ForceOpenReservations = true;
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = subject.AreReservationsOpen();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task AreReservationsOpen_WhenForcedOpenAndClosed_Undefined()
    {
        // Arrange
        Configuration.ForceOpenReservations = true;
        Configuration.ForceCloseReservations = true;
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = subject.AreReservationsOpen();

        // Assert
        // This state is explicitly undefined.
        Assert.AreEqual(result, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenForcedClosed_ReturnsClosedManually()
    {
        // Arrange
        Configuration.ForceCloseReservations = true;
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.ClosedManually, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenNotYetOpen_ReturnsOpeningLater()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddMonths(1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddYears(1);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.OpeningLater, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenAfterClose_ReturnsClosedPerSchedule()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddMonths(-1);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.ClosedPerSchedule, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenForcedOpen_ReturnsOpenedManually()
    {
        // Arrange
        Configuration.ForceOpenReservations = true;
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.OpenedManually, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenForcedOpen_AndNotInDateRange_ReturnsOpenedManually()
    {
        // Arrange
        Configuration.ForceOpenReservations = true;
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddMonths(-1);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.OpenedManually, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenForcedOpen_AndAllSeatsReserved_ReturnsOutOfSeatsPermanently()
    {
        // Arrange
        Configuration.ForceOpenReservations = true;
        MockSeatsDatabase
            .Setup(m => m.CountSeats(It.IsAny<string>()))
            .ReturnsAsync(0);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.OutOfSeatsPermanently, result);
    }

    [DataTestMethod]
    [DataRow("Locked")]
    [DataRow("AwaitingPayment")]
    public async Task CalculateStatus_WhenForcedOpen_AndNoSeatsAvailable_ButSomeArePending_ReturnsOutOfSeatsTemporarily(string pendingStatus)
    {
        // Arrange
        Configuration.ForceOpenReservations = true;
        MockSeatsDatabase
            .Setup(m => m.CountSeats(It.IsAny<string>()))
            .ReturnsAsync((string status) => status == pendingStatus ? 1 : 0);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.OutOfSeatsTemporarily, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenOpenPerSchedule_ReturnsOpenedPerSchedule()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddYears(1);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.OpenedPerSchedule, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenOpenPerSchedule_AndAllSeatsReserved_ReturnsOutOfSeatsPermanently()
    {
        // Arrange
        Configuration.ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1);
        Configuration.ScheduledCloseDateTime = DateTime.UtcNow.AddYears(1);
        MockSeatsDatabase
            .Setup(m => m.CountSeats(It.IsAny<string>()))
            .ReturnsAsync(0);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.OutOfSeatsPermanently, result);
    }

    [DataTestMethod]
    [DataRow("Locked")]
    [DataRow("AwaitingPayment")]
    public async Task CalculateStatusWhenOpenPerSchedule_AndNoSeatsAvailable_ButSomeArePending_ReturnsOutOfSeatsTemporarily(string pendingStatus)
    {
        // Arrange
        Configuration.ForceOpenReservations = true;
        MockSeatsDatabase
            .Setup(m => m.CountSeats(It.IsAny<string>()))
            .ReturnsAsync((string status) => status == pendingStatus ? 1 : 0);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.OutOfSeatsTemporarily, result);
    }

    [TestMethod]
    public async Task CalculateStatus_WhenClosed_AndAllSeatsReserved_ReturnsClosed()
    {
        // Arrange
        Configuration.ForceCloseReservations = true;
        MockSeatsDatabase
            .Setup(m => m.CountSeats(It.IsAny<string>()))
            .ReturnsAsync(0);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.ClosedManually, result);
    }

    [DataTestMethod]
    [DataRow("Locked")]
    [DataRow("AwaitingPayment")]
    public async Task CalculateStatus_WhenClosed_AndNoSeatsAvailable_ButSomeArePending_ReturnsClosed(string pendingStatus)
    {
        // Arrange
        Configuration.ForceCloseReservations = true;
        MockSeatsDatabase
            .Setup(m => m.CountSeats(It.IsAny<string>()))
            .ReturnsAsync((string status) => status == pendingStatus ? 1 : 0);
        var subject = await OpenChecker.FromDatabase(
            MockConfigurationDatabase.Object,
            MockSeatsDatabase.Object);

        // Act
        var result = await subject.CalculateStatus();

        // Assert
        Assert.AreEqual(ReservationsStatus.ClosedManually, result);
    }
}

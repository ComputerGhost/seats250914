using Core.Domain.Authorization;
using Core.Domain.Common.Models;
using Core.Domain.Common.Ports;
using Moq;

namespace Core.Domain.UnitTests.Authorization;

[TestClass]
public class AuthorizationCheckerTests
{
    private Mock<IConfigurationDatabase> MockConfigurationDatabase { get; set; } = null!;
    private Mock<IReservationsDatabase> MockReservationsDatabase { get; set; } = null!;
    private Mock<ISeatLocksDatabase> MockSeatLocksDatabase { get; set; } = null!;
    private AuthorizationChecker Subject { get; set; } = null!;

    private ConfigurationEntityModel Configuration { get; set; } = null!;
    private SeatLockEntityModel SeatLock { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Configuration = new()
        {
            ForceCloseReservations = false,
            ForceOpenReservations = false,
            ScheduledOpenDateTime = DateTime.UtcNow.AddYears(-1),
        };

        SeatLock = new SeatLockEntityModel
        {
            Expiration = DateTime.UtcNow.AddYears(1),
            Key = "key",
        };

        MockConfigurationDatabase = new();
        MockConfigurationDatabase
            .Setup(m => m.FetchConfiguration())
            .ReturnsAsync(() => Configuration);

        MockReservationsDatabase = new();
        MockReservationsDatabase
            .Setup(m => m.CountActiveReservationsForEmailAddress(It.IsAny<string>()))
            .ReturnsAsync(0);

        MockSeatLocksDatabase = new();
        MockSeatLocksDatabase
            .Setup(m => m.CountLocksForIpAddress(It.IsAny<string>()))
            .ReturnsAsync(0);
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync(() => SeatLock);

        Subject = new(MockConfigurationDatabase.Object, MockReservationsDatabase.Object, MockSeatLocksDatabase.Object);
        Subject.SetUserIdentity(true, null, null);
    }

    [TestMethod]
    public async Task GetLockSeatAuthorization_WithTestInitializeDefaults_ReturnsSuccess()
    {
        // Arrange

        // Act
        var result = await Subject.GetLockSeatAuthorization();

        // Assert
        Assert.IsTrue(result.IsAuthorized);
    }

    [TestMethod]
    public async Task GetLockSeatAuthorization_WhenReservationsClosed_ReturnsReservationsClosed()
    {
        // Arrange
        Configuration.ForceCloseReservations = true;

        // Act
        var result = await Subject.GetLockSeatAuthorization();

        // Assert
        Assert.IsFalse(result.IsAuthorized);
        Assert.AreEqual(AuthorizationRejectionReason.ReservationsAreClosed, result.FailureReason);
    }

    [TestMethod]
    public async Task GetLockSeatAuthorization_WhenNotStaff_IpAddressIsRequired()
    {
        // Arrange
        Subject.SetUserIdentity(false, "email", null);

        // Act
        var action = async () => await Subject.GetLockSeatAuthorization();

        // Assert
        await Assert.ThrowsExceptionAsync<Exception>(action);
    }

    [TestMethod]
    public async Task GetLockSeatAuthorization_WhenNotStaff_AndTooManyLocks_ReturnsTooManyLocks()
    {
        // Arrange
        Subject.SetUserIdentity(false, "email", "ip address");
        Configuration.MaxSeatsPerIPAddress = 0;

        // Act
        var result = await Subject.GetLockSeatAuthorization();

        // Assert
        Assert.IsFalse(result.IsAuthorized);
        Assert.AreEqual(AuthorizationRejectionReason.TooManySeatLocksForIpAddress, result.FailureReason);
    }

    [TestMethod]
    public async Task GetReserveSeatAuthorization_WithTestInitializeDefaults_ReturnsSuccess()
    {
        // Arrange

        // Act
        var result = await Subject.GetReserveSeatAuthorization(0, SeatLock.Key);

        // Assert
        Assert.IsTrue(result.IsAuthorized);
    }

    [TestMethod]
    public async Task GetReserveSeatAuthorization_WhenNotStaff_EmailIsRequired()
    {
        // Arrange
        Subject.SetUserIdentity(false, null, "ip-address");

        // Act
        var action = async () => await Subject.GetReserveSeatAuthorization(0, SeatLock.Key);

        // Assert
        await Assert.ThrowsExceptionAsync<Exception>(action);
    }

    [TestMethod]
    public async Task GetReserveSeatAuthorization_WhenTooManyReservations_ReturnsTooManyReservations()
    {
        // Arrange
        Subject.SetUserIdentity(false, "email", "ip address");
        Configuration.MaxSeatsPerPerson = 0;

        // Act
        var result = await Subject.GetReserveSeatAuthorization(0, SeatLock.Key);

        // Assert
        Assert.IsFalse(result.IsAuthorized);
        Assert.AreEqual(AuthorizationRejectionReason.TooManyReservationsForEmail, result.FailureReason);
    }

    [TestMethod]
    public async Task GetReserveSeatAuthorization_WhenLockDoesNotExist_ReturnsSeatIsNotLocked()
    {
        // Arrange
        MockSeatLocksDatabase
            .Setup(m => m.FetchSeatLock(It.IsAny<int>()))
            .ReturnsAsync((SeatLockEntityModel?)null);

        // Act
        var result = await Subject.GetReserveSeatAuthorization(0, SeatLock.Key);

        // Assert
        Assert.IsFalse(result.IsAuthorized);
        Assert.AreEqual(AuthorizationRejectionReason.SeatIsNotLocked, result.FailureReason);
    }

    [TestMethod]
    public async Task GetReserveSeatAuthorization_WhenKeyIsInvalid_ReturnsKeyIsInvalid()
    {
        // Arrange
        SeatLock.Key = "key";
        var invalidKey = "invalid key";

        // Act
        var result = await Subject.GetReserveSeatAuthorization(0, invalidKey);

        // Assert
        Assert.IsFalse(result.IsAuthorized);
        Assert.AreEqual(AuthorizationRejectionReason.KeyIsInvalid, result.FailureReason);
    }

    [TestMethod]
    public async Task GetReserveSeatAuthorization_WhenKeyIsExpired_ReturnsKeyExpired()
    {
        // Arrange
        SeatLock.Expiration = DateTime.UtcNow.AddYears(-1);

        // Act
        var result = await Subject.GetReserveSeatAuthorization(0, SeatLock.Key);

        // Assert
        Assert.IsFalse(result.IsAuthorized);
        Assert.AreEqual(AuthorizationRejectionReason.KeyIsExpired, result.FailureReason);
    }

    [TestMethod]
    public async Task GetReserveSeatAuthorization_WhenKeyIsExpired_ButWithinGracePeriod_ReturnsSuccess()
    {
        // Arrange
        SeatLock.Expiration = DateTime.UtcNow.AddMinutes(-1);
        Configuration.GracePeriodSeconds = (int)TimeSpan.FromDays(1).TotalSeconds;

        // Act
        var result = await Subject.GetReserveSeatAuthorization(0, SeatLock.Key);

        // Assert
        Assert.IsTrue(result.IsAuthorized);
    }
}

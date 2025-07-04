namespace Public.FrontendIntegrationTests.SeatSelectorTests;

[TestClass]
public class StatusNotificationTests
{
    private SeleniumWrapper _driver = null!;

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    // TODO: Add a way to reset the data status.
    // This might be a decent time to add the `AdminReserveSeatCommand`.

    [TestMethod]
    public void Alert_WhenSeatsAvailable_RendersOpen()
    {
        // ensure the form submit button is present.
    }

    [TestMethod]
    public void Alert_WhenClosedPerSchedule_RendersPermanentlyClosed()
    {
        // also ensure the form submit button is missing
    }

    [TestMethod]
    public void Alert_WhenClosedManually_RendersTemporarilyClosed()
    {
        // also ensure the form submit button is missing
    }

    [TestMethod]
    public void Alert_WhenNoSeatsAvailable_AndAllReservationsApproved_RendersPermanentlyOutOfSeats()
    {
        // also ensure the form submit button is missing
    }

    [TestMethod]
    public void Alert_WhenNoSeatsAvailable_ButSomeAreLocked_RendersTemporarilyOutOfSeats()
    {
        // also ensure the form submit button is missing
    }

    [TestMethod]
    public void Alert_WhenNoSeatsAvailable_AndSomeReservationsPending_RendersTemporarilyOutOfSeats()
    {
        // also ensure the form submit button is missing
    }

    [TestMethod]
    public void Form_WhenMaxLocksForUser_RendersMaxLocks()
    {
    }
}

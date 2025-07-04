namespace Public.FrontendIntegrationTests.ReservationTests;
public class ReservationFormTests
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

    [TestMethod]
    public void Form_WhenKeyIsInvalid_RendersKeyIsInvalid()
    {
    }

    [TestMethod]
    public void Form_WhenReservationsClosed_RendersReservationsClosed()
    {
    }

    [TestMethod]
    public void Form_WhenMaxReservationsForUser_RendersMaxReservations()
    {
    }

    [TestMethod]
    public void Form_WhenSuccess_RedirectsToPaymentPage()
    {
    }

    [TestMethod]
    public void Page_WhenExpired_RedirectsToTimeout()
    {
    }
}

using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System.Collections.ObjectModel;
using System.Web;

namespace Public.IntegrationTests.ReservationTests;

[LocalOnly]
[TestClass]
public class ReservationFormTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private ReadOnlyCollection<IWebElement> Alerts => _driver.FindElements(By.ClassName("alert"));
    private IWebElement Heading => _driver.FindElement(By.TagName("h1"));
    private IWebElement Cancel => _driver.FindElement(By.ClassName("btn-secondary"));
    private IWebElement Submit => _driver.FindElement(By.ClassName("btn-primary"));

    [TestInitialize]
    public async Task Initialize()
    {
        _driver = new SeleniumWrapper(languageId: "en");
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

        // Start with a clean slate.
        await TestDataSetup.DeleteAllReservations();
        await _mediator.Send(TestDataSetup.WorkingSaveConfigurationCommand);

        // Lock a seat and navigate to the page.
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        SetLockCookie(await LockSeat(1));
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "/reservation/new");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void Cancel_WhenClicked_ReturnsToHome()
    {
        // Act 1: Click cancel
        _driver.ScrollTo(Cancel);
        Cancel.Click();

        // Act 2: Wait for page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == "/");

        // Assert
        Assert.AreEqual("Fall in Hyelin", Heading.Text);
    }

    [TestMethod]
    public void Page_WhenValidLoad_RendersNormally()
    {
        // Assert
        Assert.AreEqual(0, Alerts.Count);
    }

    [TestMethod]
    public async Task Submit_WhenKeyIsInvalid_RendersKeyIsInvalid()
    {
        // Arrange 1: Set up an invalid key
        var seatLock =await LockSeat(2);
        seatLock.SeatKey = "invalid";
        SetLockCookie(seatLock);

        // Act 1: Refresh the page, but it trusts the user key at first.
        _driver.Navigate().Refresh();
        Assert.AreEqual(0, Alerts.Count);

        // Act 2: Submit the form to get the error.
        PopulateForm();
        _driver.ScrollTo(Submit);
        Submit.Click();

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.IsTrue(Alerts[0].Text.Contains("wrong key"));
    }

    [TestMethod]
    public async Task Submit_WhenSeatIsWrong_RendersKeyIsInvalid()
    {
        // Arrange 1: Set up an invalid key
        var seatLock = await LockSeat(2);
        seatLock.SeatNumber = 1;
        SetLockCookie(seatLock);

        // Act 1: Refresh the page, but it trusts the user key at first.
        _driver.Navigate().Refresh();
        Assert.AreEqual(0, Alerts.Count);

        // Act 2: Submit the form to get the error.
        PopulateForm();
        _driver.ScrollTo(Submit);
        Submit.Click();

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.IsTrue(Alerts[0].Text.Contains("wrong key"));
    }

    [TestMethod]
    public async Task Page_WhenReservationsClosed_RendersReservationsClosed()
    {
        // Arrange
        var saveConfigurationCommand = TestDataSetup.WorkingSaveConfigurationCommand;
        saveConfigurationCommand.ForceCloseReservations = true;
        saveConfigurationCommand.ForceOpenReservations = false;
        await _mediator.Send(saveConfigurationCommand);

        // Act: Submit the form to get the error.
        PopulateForm();
        _driver.ScrollTo(Submit);
        Submit.Click();

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.IsTrue(Alerts[0].Text.Contains("Reservations have been closed."));
    }

    [TestMethod]
    public async Task Page_WhenMaxReservationsForUser_RendersMaxReservations()
    {
        // Arrange
        var saveConfigurationCommand = TestDataSetup.WorkingSaveConfigurationCommand;
        saveConfigurationCommand.MaxSeatsPerPerson = 0;
        await _mediator.Send(saveConfigurationCommand);

        // Act 1: Refresh the page, but it doesn't know the user yet.
        _driver.Navigate().Refresh();
        Assert.AreEqual(0, Alerts.Count);

        // Act 2: Submit the form to get the error.
        PopulateForm();
        _driver.ScrollTo(Submit);
        Submit.Click();

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.IsTrue(Alerts[0].Text.Contains("Too many reservations"));
    }

    [TestMethod]
    public async Task Page_WhenExpired_RedirectsToTimeout()
    {
        // Arrange 1: Expiration config
        var saveConfigurationCommand = TestDataSetup.WorkingSaveConfigurationCommand;
        saveConfigurationCommand.MaxSecondsToConfirmSeat = 0;
        saveConfigurationCommand.GracePeriodSeconds = 0;
        await _mediator.Send(saveConfigurationCommand);

        // Arrange 2: Expired lock
        SetLockCookie(await LockSeat(2));

        // Act
        _driver.Navigate().Refresh();

        // Assert
        Assert.AreEqual("Fall in Hyelin", Heading.Text);
    }

    [TestMethod]
    public void Submit_WhenClicked_AndFormValid_RedirectsToPaymentPage()
    {
        // Arrange
        const string EXPECTED_TITLE = "";

        // Act 1: Submit the form
        PopulateForm();
        _driver.ScrollTo(Submit);
        Submit.Click();

        // Act 2: Wait for the payment page to process.
        _driver.WaitUntil(_ => _driver.Title == EXPECTED_TITLE, maxSeconds: 4);

        // Assert
        Assert.AreEqual(EXPECTED_TITLE, _driver.Title);
    }

    private async Task<LockSeatCommandResponse> LockSeat(int seatNumber)
    {
        var seatLock = await _mediator.Send(new LockSeatCommand
        {
            IpAddress = "-",
            SeatNumber = seatNumber,
        });
        return seatLock.Value;
    }

    private void PopulateForm()
    {
        var name = _driver.FindElement(By.Id("Name"));
        var email = _driver.FindElement(By.Id("Email"));
        var agreeToTerms = _driver.FindElement(By.Id("AgreeToTerms"));

        name.SendKeys("Bob");
        email.SendKeys("bob@example.com");
        _driver.ScrollTo(agreeToTerms);
        agreeToTerms.Click();
    }

    private void SetLockCookie(LockSeatCommandResponse seatLock)
    {
        var serialized = JsonConvert.SerializeObject(seatLock);
        var encoded = HttpUtility.UrlEncode(serialized);
        var cookie = new Cookie("seatLock", encoded, "/", seatLock.LockExpiration.UtcDateTime);
        _driver.Manage().Cookies.AddCookie(cookie);
    }
}

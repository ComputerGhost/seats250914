using Core.Application.Reservations;
using Core.Application.Seats;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace Public.IntegrationTests.SeatSelectorTests;

[LocalOnly]
[TestClass]
public class StatusNotificationTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    // Root and above the seat map
    private IWebElement Section => _driver.FindElement(By.Id("reserve-seats"));
    private ReadOnlyCollection<IWebElement> Alerts => Section.FindElements(By.ClassName("alert"));

    // Seat map
    private IWebElement AvailableSeat => Section.FindElement(By.CssSelector(".audience .available"));

    // Form on right side
    private ReadOnlyCollection<IWebElement> Selects => Section.FindElements(By.ClassName("form-select"));
    private IWebElement Submit => Section.FindElement(By.ClassName("btn-primary"));
    private IWebElement ValidationMessage => Section.FindElement(By.ClassName("text-danger"));


    [TestInitialize]
    public async Task Initialize()
    {
        _driver = new SeleniumWrapper(languageId: "en");
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

        // Start with a clean slate.
        await TestDataSetup.DeleteAllReservations();
        await _mediator.Send(TestDataSetup.WorkingSaveConfigurationCommand);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void Alert_WhenSeatsAvailable_RendersOpen()
    {
        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "#reserve-seats");

        // Assert
        Assert.AreEqual(0, Alerts.Count);
        Assert.IsTrue(Selects.FirstOrDefault()?.Displayed ?? false);
    }

    [TestMethod]
    public async Task Alert_WhenClosedPerSchedule_RendersPermanentlyClosed()
    {
        // Arrange
        var saveConfigurationCommand = TestDataSetup.WorkingSaveConfigurationCommand;
        saveConfigurationCommand.ForceOpenReservations = false;
        saveConfigurationCommand.ScheduledOpenDateTime = DateTime.Now.AddDays(-2);
        saveConfigurationCommand.ScheduledCloseDateTime = DateTime.Now.AddDays(-1);
        await _mediator.Send(saveConfigurationCommand);

        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "#reserve-seats");

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.AreEqual("Reservations are closed!", Alerts[0].FindElement(By.TagName("h3")).Text);
        Assert.IsFalse(Selects.FirstOrDefault()?.Displayed ?? false);
    }

    [TestMethod]
    public async Task Alert_WhenClosedManually_RendersTemporarilyClosed()
    {
        // Arrange
        var saveConfigurationCommand = TestDataSetup.WorkingSaveConfigurationCommand;
        saveConfigurationCommand.ForceCloseReservations = true;
        saveConfigurationCommand.ForceOpenReservations = false;
        await _mediator.Send(saveConfigurationCommand);

        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "#reserve-seats");

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.AreEqual("Reservations are closed!", Alerts[0].FindElement(By.TagName("h3")).Text);
        Assert.IsFalse(Selects.FirstOrDefault()?.Displayed ?? false);
    }

    [TestMethod]
    public async Task Alert_WhenNoSeatsAvailable_AndAllReservationsApproved_RendersPermanentlyOutOfSeats()
    {
        // Arrange
        var listSeatsResponse = await _mediator.Send(new ListSeatsQuery());
        foreach (var seat in listSeatsResponse.Data)
        {
            await _mediator.Send(new AdminReserveSeatCommand
            {
                SeatNumber = seat.SeatNumber,
                Name = "bob",
                Email = "bob@example.com",
                PreferredLanguage = "English",
            });
        }

        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "#reserve-seats");

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.AreEqual("We're out of seats!", Alerts[0].FindElement(By.TagName("h3")).Text);
        Assert.IsFalse(Alerts[0].FindElement(By.TagName("p")).Text.Contains("check back later"));
        Assert.IsFalse(Selects.FirstOrDefault()?.Displayed ?? false);
    }

    [TestMethod]
    public async Task Alert_WhenNoSeatsAvailable_ButSomeAreLocked_RendersTemporarilyOutOfSeats()
    {
        // Arrange
        var listSeatsResponse = await _mediator.Send(new ListSeatsQuery());
        foreach (var seat in listSeatsResponse.Data)
        {
            await _mediator.Send(new LockSeatCommand
            {
                IpAddress = "-",
                SeatNumber = seat.SeatNumber,
            });
        }

        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "#reserve-seats");

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.AreEqual("We're out of seats!", Alerts[0].FindElement(By.TagName("h3")).Text);
        Assert.IsTrue(Alerts[0].FindElement(By.TagName("p")).Text.Contains("check back later"));
        Assert.IsFalse(Selects.FirstOrDefault()?.Displayed ?? false);
    }

    [TestMethod]
    public async Task Alert_WhenNoSeatsAvailable_AndSomeReservationsPending_RendersTemporarilyOutOfSeats()
    {
        // Arrange
        var listSeatsResponse = await _mediator.Send(new ListSeatsQuery());
        foreach (var seat in listSeatsResponse.Data)
        {
            await _mediator.Send(new LockSeatCommand
            {
                IpAddress = "-",
                SeatNumber = seat.SeatNumber,
            });
        }

        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "#reserve-seats");

        // Assert
        Assert.AreEqual(1, Alerts.Count);
        Assert.AreEqual("We're out of seats!", Alerts[0].FindElement(By.TagName("h3")).Text);
        Assert.IsTrue(Alerts[0].FindElement(By.TagName("p")).Text.Contains("check back later"));
        Assert.IsFalse(Selects.FirstOrDefault()?.Displayed ?? false);
    }

    [TestMethod]
    public async Task Form_WhenMaxLocksForIpAddress_RendersMaxLocks()
    {
        // Arrange
        var saveConfigurationCommand = TestDataSetup.WorkingSaveConfigurationCommand;
        saveConfigurationCommand.MaxSeatsPerIPAddress = 0;
        await _mediator.Send(saveConfigurationCommand);

        // Act 1: Try to lock a seat
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "#reserve-seats");
        _driver.ScrollTo(AvailableSeat);
        AvailableSeat.Click();
        Submit.Click();

        // Act 2: Wait for it to process
        _driver.WaitUntil(_ => ValidationMessage.Displayed);

        // Assert
        Assert.IsTrue(ValidationMessage.Displayed);
        Assert.AreEqual("Too many reservations have been submitted from your IP address.", ValidationMessage.Text);
    }
}

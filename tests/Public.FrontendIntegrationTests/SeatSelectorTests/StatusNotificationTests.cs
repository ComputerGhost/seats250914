using Core.Application.Reservations;
using Core.Application.Seats;
using Core.Application.System;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace Public.FrontendIntegrationTests.SeatSelectorTests;

[LocalOnly]
[TestClass]
public class StatusNotificationTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private static SaveConfigurationCommand WorkingSaveConfigurationCommand => new()
    {
        ForceCloseReservations = false,
        ForceOpenReservations = true,
        MaxSeatsPerIPAddress = int.MaxValue,
        MaxSeatsPerPerson = int.MaxValue,
        MaxSecondsToConfirmSeat = 3600,
        ScheduledCloseTimeZone = "UTC",
        ScheduledOpenTimeZone = "UTC",
    };

    private IWebElement Section => _driver.FindElement(By.Id("reserve-seats"));
    private ReadOnlyCollection<IWebElement> Alerts => Section.FindElements(By.ClassName("alert"));
    private ReadOnlyCollection<IWebElement> Selects => Section.FindElements(By.ClassName("form-select"));
    private IWebElement AvailableSeat => Section.FindElement(By.CssSelector(".audience .available"));
    private IWebElement Submit => Section.FindElement(By.ClassName("btn-primary"));

    [TestInitialize]
    public async Task Initialize()
    {
        _driver = new SeleniumWrapper(languageId: "en");
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

        // Start with a clean slate.
        await _mediator.Send(new DeleteAllReservationDataCommand());
        await _mediator.Send(WorkingSaveConfigurationCommand);
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
        var saveConfigurationCommand = WorkingSaveConfigurationCommand;
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
        var saveConfigurationCommand = WorkingSaveConfigurationCommand;
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
    public async Task Form_WhenMaxLocksForUser_RendersMaxLocks()
    {
        // Arrange
        var saveConfigurationCommand = WorkingSaveConfigurationCommand;
        saveConfigurationCommand.MaxSeatsPerIPAddress = 0;
        await _mediator.Send(saveConfigurationCommand);

        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "#reserve-seats");
        _driver.ScrollTo(AvailableSeat);
        AvailableSeat.Click();
        Submit.Click();

        // Assert
        var errorMessage = Section.FindElement(By.ClassName("text-danger"));
        Assert.IsTrue(errorMessage.Displayed);
        Assert.AreEqual("Too many reservations have been submitted from your IP address.", errorMessage.Text);
    }
}

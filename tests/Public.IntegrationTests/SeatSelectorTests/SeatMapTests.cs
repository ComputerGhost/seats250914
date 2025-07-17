using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Public.IntegrationTests.SeatSelectorTests;

[TestClass]
public class SeatMapTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement Section => _driver.FindElement(By.Id("reserve-seats"));
    private IWebElement AvailableSeat => Section.FindElement(By.CssSelector(".audience .available"));
    private IWebElement ReservedSeat => Section.FindElement(By.CssSelector(".audience .reserved"));
    private IList<SelectElement> Dropdowns => [.. Section.FindElements(By.ClassName("form-select")).Select(x => new SelectElement(x))];

    [TestInitialize]
    public async Task Initialize()
    {
        _driver = new SeleniumWrapper();
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
    public void AvailableSeats_WhenClicked_SelectsSeats()
    {
        // Arrange
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);

        // Act: Click the first seat
        var seat1 = AvailableSeat;
        _driver.ScrollTo(seat1);
        seat1.Click();

        // Act: Click the second seat
        var seat2 = AvailableSeat;
        _driver.ScrollTo(seat2);
        seat2.Click();

        // Assert
        Assert.IsTrue(seat1.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual(seat1.Text, Dropdowns[0].SelectedOption.Text);
        Assert.IsTrue(seat2.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual(seat2.Text, Dropdowns[1].SelectedOption.Text);
    }

    [TestMethod]
    public void SelectedSeat_WhenClicked_DeselectsSeat()
    {
        // Arrange
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        var availableSeat = AvailableSeat;
        _driver.ScrollTo(availableSeat);
        availableSeat.Click();

        // Act: Unclick the first seat.
        availableSeat.Click();

        // Assert
        Assert.IsFalse(availableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual("", Dropdowns[0].SelectedOption.Text);
    }

    [TestMethod]
    public async Task UnavailableSeat_WhenClicked_DoesNotSelectSeat()
    {
        // Arrange
        await _mediator.Send(new AdminReserveSeatCommand
        {
            SeatNumber = 1,
            Name = "bob",
            Email = "bob@example.com",
            PreferredLanguage = "English",
        });
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        var unavailableSeat = ReservedSeat;

        // Act
        _driver.ScrollTo(unavailableSeat);
        unavailableSeat.Click();

        // Assert
        Assert.IsFalse(unavailableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual("", Dropdowns[0].SelectedOption.Text);
    }

    [DataTestMethod]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public async Task AvailableSeat_WhenOverSeatLimit_DeselectsFirstSeat(int maxSeats)
    {
        // Arrange: Save new config.
        var configuration = TestDataSetup.WorkingSaveConfigurationCommand;
        configuration.MaxSeatsPerReservation = maxSeats;
        await _mediator.Send(configuration);

        // Arrange: Navigate to page.
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        var firstSeat = AvailableSeat;

        // Act: Select one more than the max seats.
        _driver.ScrollTo(firstSeat);
        firstSeat.Click();
        for (int i = 1; i != maxSeats + 1; ++i)
        {
            var nextSeat = AvailableSeat;
            _driver.ScrollTo(nextSeat);
            nextSeat.Click();
        }

        // Assert
        Assert.IsFalse(firstSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreNotEqual(firstSeat.Text, Dropdowns[0].SelectedOption.Text);
    }
}

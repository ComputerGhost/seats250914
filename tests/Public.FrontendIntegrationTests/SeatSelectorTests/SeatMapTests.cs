using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Public.FrontendIntegrationTests.SeatSelectorTests;

[TestClass]
public class SeatMapTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement Section => _driver.FindElement(By.Id("reserve-seats"));
    private IWebElement AvailableSeat => Section.FindElement(By.CssSelector(".audience .available"));
    private IWebElement ReservedSeat => Section.FindElement(By.CssSelector(".audience .reserved"));
    private SelectElement Dropdown => new(Section.FindElement(By.ClassName("form-select")));

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
    public void AvailableSeat_WhenClicked_SelectsSeat()
    {
        // Arrange
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        var availableSeat = AvailableSeat;

        // Act
        _driver.ScrollTo(availableSeat);
        availableSeat.Click();

        // Assert
        Assert.IsTrue(availableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual(availableSeat.Text, Dropdown.SelectedOption.Text);
    }

    [TestMethod]
    public void SelectedSeat_WhenClicked_DeselectsSeat()
    {
        // Arrange
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        var availableSeat = AvailableSeat;
        _driver.ScrollTo(availableSeat);
        availableSeat.Click(); // Make the seat selected

        // Act
        availableSeat.Click();

        // Assert
        Assert.IsFalse(availableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual("", Dropdown.SelectedOption.Text);
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
        Assert.AreEqual("", Dropdown.SelectedOption.Text);
    }

    [TestMethod]
    public void AvailableSeat_WhenOverSeatLimit_DeselectsFirstSeat()
    {
        // Arrange
        const int MAX_SEATS = 1;
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        var firstSeat = AvailableSeat;

        // Act
        _driver.ScrollTo(firstSeat);
        firstSeat.Click();
        for (int i = 1; i != MAX_SEATS + 1; ++i)
        {
            var nextSeat = AvailableSeat;
            _driver.ScrollTo(nextSeat);
            nextSeat.Click();
        }

        // Assert
        Assert.IsFalse(firstSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreNotEqual(firstSeat.Text, Dropdown.SelectedOption.Text);
    }
}

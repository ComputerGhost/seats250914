using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Public.IntegrationTests.SeatSelectorTests;

[TestClass]
public class LiveUpdatesTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement Section => _driver.FindElement(By.Id("reserve-seats"));
    private IWebElement AvailableSeat => Section.FindElement(By.CssSelector(".audience .available"));
    private IWebElement Submit => Section.FindElement(By.ClassName("btn-primary"));

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
    public void AvailableSeat_WhenOtherUserLocks_BecomesUnavailable()
    {
        // Arrange
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        var availableSeat = AvailableSeat;

        // Act: Reserve seat as other user
        _driver.SwitchTo().NewWindow(WindowType.Tab);
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        _driver.ScrollTo(AvailableSeat);
        AvailableSeat.Click();
        _driver.ScrollTo(Submit);
        Submit.Click();

        // Act: Switch back to first tab and wait for UI update
        _driver.SwitchTo().Window(_driver.WindowHandles[0]);
        _driver.WaitUntil(d => availableSeat.GetAttribute("class") == "seat on-hold");

        // Assert
        Assert.AreEqual("seat on-hold", availableSeat.GetAttribute("class"));
    }
}

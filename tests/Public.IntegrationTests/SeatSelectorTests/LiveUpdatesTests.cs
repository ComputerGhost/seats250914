using Core.Application.Reservations;
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
    private SelectElement Select => new(Section.FindElement(By.ClassName("form-select")));

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
        _mediator.Send(new LockSeatsCommand
        {
            IpAddress = "-",
            SeatNumbers = [int.Parse(availableSeat.Text)],
        });

        // Act: Wait for UI update.
        _driver.WaitUntil(d => availableSeat.GetAttribute("class") == ".seat .on-hold");

        // Assert
        Assert.AreEqual(".seat .on-hold", availableSeat.GetAttribute("class"));
        var options = Select.Options.Skip(1).Select(x => x.Text);
        Assert.DoesNotContain(availableSeat.Text, options);
    }
}

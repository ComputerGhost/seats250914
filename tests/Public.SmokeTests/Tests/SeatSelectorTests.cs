using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Public.SmokeTests.Extensions;
using Public.SmokeTests.Utilities;

namespace Public.SmokeTests.Tests;

[TestClass]
[SmokeTest("These tests will work on in any environment on a running website instance.")]
public class SeatSelectorTests : TestBase
{
    private IWebDriver Driver { get; set; } = null!;
    private IWebElement SeatMap { get; set; } = null!;
    private SelectElement Dropdown { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Driver = CreateDriver("en");
        Driver.Navigate().GoToUrl(TargetUrl);

        var section = Driver.FindElement(By.Id("reserve-a-seat"));
        SeatMap = section.FindElement(By.ClassName("audience"));
        Dropdown = new SelectElement(section.FindElement(By.ClassName("form-select")));
    }

    [TestCleanup]
    public void Cleanup()
    {
        Driver.Quit();
        Driver.Dispose();
    }

    [TestMethod]
    public void WhenAvailableSeatClicked_SelectsSeat()
    {
        // Arrange
        var availableSeat = SeatMap.FindElement(By.ClassName("available"));

        // Act
        Driver.ScrollToAndClick(availableSeat);

        // Assert
        Assert.IsTrue(availableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual(availableSeat.Text, Dropdown.SelectedOption.Text);
    }

    [TestMethod]
    public void WhenSelectedSeatClicked_DeselectsSeat()
    {
        // Arrange
        var availableSeat = SeatMap.FindElement(By.ClassName("available"));
        Driver.ScrollToAndClick(availableSeat);

        // Act
        Driver.ScrollToAndClick(availableSeat);

        // Assert
        Assert.IsFalse(availableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual("", Dropdown.SelectedOption.Text);
    }

    [TestMethod]
    public void WhenUnavailableSeatClicked_DoesNotSelectSeat()
    {
        // Arrange
        var unavailableSeat = SeatMap.FindElement(By.ClassName("reserved"));

        // Act
        Driver.ScrollToAndClick(unavailableSeat);

        // Assert
        Assert.IsFalse(unavailableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual("", Dropdown.SelectedOption.Text);
    }

    [TestMethod]
    public void WhenOverSeatLimit_AndAdditionalSeatSelected_DeselectsFirstSeat()
    {
        // Arrange
        const int MAX_SEATS = 1;
        var firstSeat = SeatMap.FindElement(By.ClassName("available"));

        // Arrange
        Driver.ScrollToAndClick(firstSeat);
        for (int i = 1; i != MAX_SEATS + 1; ++i)
        {
            var nextSeat = SeatMap.FindElement(By.ClassName("available"));
            Driver.ScrollToAndClick(nextSeat);
        }

        // Assert
        Assert.IsFalse(firstSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreNotEqual(firstSeat.Text, Dropdown.SelectedOption.Text);
    }
}

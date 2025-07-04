using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Public.FrontendIntegrationTests.SeatSelectorTests;

[TestClass]
public class SeatMapTests
{
    private SeleniumWrapper _driver = null!;
    private IWebElement _seatMap = null!;
    private SelectElement _dropdown = null!;

    private IWebElement AvailableSeat => _seatMap.FindElement(By.ClassName("available"));
    private IWebElement ReservedSeat => _seatMap.FindElement(By.ClassName("reserved"));

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper();
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);

        var section = _driver.FindElement(By.Id("reserve-a-seat"));
        _seatMap = section.FindElement(By.ClassName("audience"));
        _dropdown = new SelectElement(section.FindElement(By.ClassName("form-select")));
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
        var availableSeat = AvailableSeat;

        // Act
        availableSeat.Click();

        // Assert
        Assert.IsTrue(availableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual(availableSeat.Text, _dropdown.SelectedOption.Text);
    }

    [TestMethod]
    public void SelectedSeat_WhenClicked_DeselectsSeat()
    {
        // Arrange
        var availableSeat = AvailableSeat;
        availableSeat.Click(); // Make the seat selected

        // Act
        availableSeat.Click();

        // Assert
        Assert.IsFalse(availableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual("", _dropdown.SelectedOption.Text);
    }

    [TestMethod]
    public void UnavailableSeat_WhenClicked_DoesNotSelectSeat()
    {
        // Arrange
        var unavailableSeat = ReservedSeat;

        // Act
        unavailableSeat.Click();

        // Assert
        Assert.IsFalse(unavailableSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreEqual("", _dropdown.SelectedOption.Text);
    }

    [TestMethod]
    public void AvailableSeat_WhenOverSeatLimit_DeselectsFirstSeat()
    {
        // Arrange
        const int MAX_SEATS = 1;
        var firstSeat = AvailableSeat;

        // Act
        firstSeat.Click();
        for (int i = 1; i != MAX_SEATS + 1; ++i)
        {
            AvailableSeat.Click();
        }

        // Assert
        Assert.IsFalse(firstSeat.GetAttribute("class")?.Contains("selected"));
        Assert.AreNotEqual(firstSeat.Text, _dropdown.SelectedOption.Text);
    }
}

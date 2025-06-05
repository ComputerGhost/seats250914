using OpenQA.Selenium;

namespace Public.SmokeTests.Tests;

[TestClass]
public sealed class KoreanLocalizationTests : TestBase
{
    private IWebDriver Driver { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Driver = CreateDriver("ko");
    }

    [TestCleanup]
    public void Cleanup()
    {
        Driver.Quit();
        Driver.Dispose();
    }

    [TestMethod]
    public void WhenVisitingWebsiteForFirstTime_WebsiteRendersInEnglish()
    {
        // Arrange

        // Act
        Driver.Navigate().GoToUrl(TargetUrl);

        // Assert
        Assert.AreEqual("혜린 팬미팅 2025", Driver.Title);
    }

    [TestMethod]
    public void WhenKoreanSelected_WebsiteRendersInKorean()
    {
        // Arrange
        Driver.Navigate().GoToUrl(TargetUrl);
        var switcher = Driver.FindElement(By.Id("culture-switcher"));
        var dropdown = switcher.FindElement(By.ClassName("dropdown-toggle"));
        dropdown.Click();
        var koreanButton = switcher.FindElement(By.XPath(".//button[text()='English']"));
        koreanButton.Click();

        // Act
        Driver.Navigate().GoToUrl(TargetUrl);

        // Assert
        Assert.AreEqual("Hyelin Fanmeeting 2025", Driver.Title);
    }
}

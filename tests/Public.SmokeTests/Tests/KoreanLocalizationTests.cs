using OpenQA.Selenium;
using Public.SmokeTests.Utilities;

namespace Public.SmokeTests.Tests;

[TestClass]
[SmokeTest("These tests will work on in any environment on a running website instance.")]
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
    public void WhenVisitingWebsiteForFirstTime_WebsiteRendersInKorean()
    {
        // Arrange

        // Act
        Driver.Navigate().GoToUrl(TargetUrl);

        // Assert
        Assert.AreEqual("혜린 팬미팅 2025", Driver.Title);
    }

    [TestMethod]
    public void WhenEnglishSelected_WebsiteRendersInKorean()
    {
        // Arrange
        Driver.Navigate().GoToUrl(TargetUrl);
        var switcher = Driver.FindElement(By.Id("culture-switcher"));
        var dropdown = switcher.FindElement(By.ClassName("dropdown-toggle"));
        dropdown.Click();
        var englishButton = switcher.FindElement(By.XPath(".//button[text()='English']"));
        englishButton.Click();

        // Act
        Driver.Navigate().GoToUrl(TargetUrl);

        // Assert
        Assert.AreEqual("Hyelin Fanmeeting 2025", Driver.Title);
    }
}

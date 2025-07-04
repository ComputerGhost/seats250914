using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace Public.FrontendIntegrationTests.NavBarTests;

[TestClass]
public class CultureSwitcherTests
{
    private SeleniumWrapper? _driver;

    private IWebElement DropdownToggle => _driver!
        .FindElement(By.Id("culture-switcher"))
        .FindElement(By.ClassName("dropdown-toggle"));
    private ReadOnlyCollection<IWebElement> DropdownOptions => _driver!
        .FindElement(By.Id("culture-switcher"))
        .FindElements(By.ClassName("dropdown-item"));


    [TestCleanup]
    public void Cleanup()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }

    [TestMethod]
    public void CultureSwitcher_WhenEnglishBrowser_CanSwapToKoreanAndBack()
    {
        // Arrange
        _driver = new SeleniumWrapper(languageId: "en");
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);

        // Assert initial state
        Assert.AreEqual("Hyelin Fanmeeting 2025", _driver.Title);
        Assert.IsTrue(DropdownToggle.Displayed);
        Assert.AreEqual(2, DropdownOptions.Count);
        Assert.IsFalse(DropdownOptions[0].Displayed);

        // Act 1: Swap to Korean
        DropdownToggle.Click();
        Assert.IsTrue(DropdownOptions[1].Displayed);
        DropdownOptions[1].Click();
        Assert.AreEqual("혜린 팬미팅 2025", _driver.Title);

        // Act 2: Swap back to English
        DropdownToggle.Click();
        Assert.IsTrue(DropdownOptions[0].Displayed);
        DropdownOptions[0].Click();
        Assert.AreEqual("Hyelin Fanmeeting 2025", _driver.Title);
    }

    [TestMethod]
    public void CultureSwitcher_WhenKoreanBrowser_CanSwapToEnglishAndBack()
    {
        // Arrange
        _driver = new SeleniumWrapper(languageId: "ko");
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);

        // Assert initial state
        Assert.AreEqual("혜린 팬미팅 2025", _driver.Title);
        Assert.IsTrue(DropdownToggle.Displayed);
        Assert.AreEqual(2, DropdownOptions.Count);
        Assert.IsFalse(DropdownOptions[0].Displayed);

        // Act 1: Swap to English
        DropdownToggle.Click();
        Assert.IsTrue(DropdownOptions[0].Displayed);
        DropdownOptions[0].Click();
        Assert.AreEqual("Hyelin Fanmeeting 2025", _driver.Title);

        // Act 1: Swap back to Korean
        DropdownToggle.Click();
        Assert.IsTrue(DropdownOptions[1].Displayed);
        DropdownOptions[1].Click();
        Assert.AreEqual("혜린 팬미팅 2025", _driver.Title);
    }
}

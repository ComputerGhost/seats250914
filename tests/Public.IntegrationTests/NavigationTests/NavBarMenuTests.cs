using OpenQA.Selenium;
using System.Collections.ObjectModel;

namespace Public.IntegrationTests.NavigationTests;

[TestClass]
public class NavBarMenuTests
{
    private SeleniumWrapper _driver = null!;

    private IWebElement NavBarMenu => _driver.FindElement(By.Id("navbar-menu"));
    private IWebElement MenuToggle => _driver.FindElement(By.ClassName("navbar-toggler"));
    private ReadOnlyCollection<IWebElement> NavLinks => NavBarMenu.FindElements(By.ClassName("nav-link"));

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void Menu_WhenDesktop_IsNotCollapsible()
    {
        // Arrange
        _driver.ResizeToDesktop();
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);

        // Assert initial state
        Assert.IsFalse(MenuToggle.Displayed);
        Assert.IsTrue(NavBarMenu.Displayed);
    }

    [TestMethod]
    public void Menu_WhenMobile_IsCollapsible()
    {
        // Arrange
        _driver.ResizeToMobile();
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);

        // Assert initial state
        Assert.IsTrue(MenuToggle.Displayed);
        Assert.IsFalse(NavBarMenu.Displayed);

        // Act 1: Click to expand
        MenuToggle.Click();
        Assert.IsTrue(MenuToggle.Displayed);
        Assert.IsTrue(NavBarMenu.Displayed);

        // Act 2: Click to collapse
        MenuToggle.Click();
        Assert.IsTrue(MenuToggle.Displayed);
        Assert.IsTrue(NavBarMenu.Displayed);
    }

    [TestMethod]
    public void NavItem_WhenScrolledToSection_IsHighlighted()
    {
        // Arrange
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);

        // Assert initial state
        Assert.IsTrue(NavBarMenu.Displayed);
        Assert.AreEqual(4, NavLinks.Count);
        Assert.IsTrue(NavLinks[0].GetAttribute("class")!.EndsWith("active"));

        // Act 1: Go to top
        ScrollTo("top");
        Assert.IsTrue(_driver.WaitUntil(_ => NavLinks[0].GetAttribute("class")!.EndsWith("active")));

        // Act 2: Go to venue info
        ScrollTo("venue-info");
        Assert.IsTrue(_driver.WaitUntil(_ => NavLinks[1].GetAttribute("class")!.EndsWith("active")));

        // Act 3: Go to event info
        ScrollTo("event-info");
        Assert.IsTrue(_driver.WaitUntil(_ => NavLinks[2].GetAttribute("class")!.EndsWith("active")));

        // Act 4: Go to reserve a seat
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        ScrollTo("reserve-seats");
        Assert.IsTrue(_driver.WaitUntil(_ => NavLinks[3].GetAttribute("class")!.EndsWith("active")));

        void ScrollTo(string fragment)
        {
            var uriBuilder = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl);
            uriBuilder.Fragment = fragment;
            _driver.Navigate().GoToUrl(uriBuilder.ToString());
        }
    }
}

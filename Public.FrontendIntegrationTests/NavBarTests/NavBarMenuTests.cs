using OpenQA.Selenium;

namespace Public.FrontendIntegrationTests.NavBarTests;

[TestClass]
public class NavBarMenuTests
{
    private SeleniumWrapper _driver = null!;

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
        var menu = _driver.FindElement(By.Id("navbar-menu"));
        var menuToggler = _driver.FindElement(By.ClassName("navbar-toggler"));

        // Assert initial state
        Assert.IsFalse(menuToggler.Displayed);
        Assert.IsTrue(menu.Displayed);
    }

    [TestMethod]
    public void Menu_WhenMobile_IsCollapsible()
    {
        // Arrange
        _driver.ResizeToMobile();
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        var menu = _driver.FindElement(By.Id("navbar-menu"));
        var menuToggler = _driver.FindElement(By.ClassName("navbar-toggler"));

        // Assert initial state
        Assert.IsTrue(menuToggler.Displayed);
        Assert.IsFalse(menu.Displayed);

        // Act 1: Click to expand
        menuToggler.Click();
        Assert.IsTrue(menuToggler.Displayed);
        Assert.IsTrue(menu.Displayed);

        // Act 2: Click to collapse
        menuToggler.Click();
        Assert.IsTrue(menuToggler.Displayed);
        Assert.IsTrue(menu.Displayed);
    }

    [TestMethod]
    public void NavItem_WhenScrolledToSection_IsHighlighted()
    {
        // Arrange
        var uriBuilder = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl);
        _driver.Navigate().GoToUrl(uriBuilder.ToString());
        var menu = _driver.FindElement(By.Id("navbar-menu"));
        var navLinks = menu.FindElements(By.ClassName("nav-link"));

        // Assert initial state
        Assert.IsTrue(menu.Displayed);
        Assert.AreEqual(4, navLinks.Count);
        Assert.IsTrue(navLinks[0].GetAttribute("class")!.EndsWith("active"));

        // Act 1: Go to top
        uriBuilder.Fragment = "#top";
        _driver.Navigate().GoToUrl(uriBuilder.ToString());
        Assert.IsTrue(navLinks[0].GetAttribute("class")!.EndsWith("active"));

        // Act 2: Go to venue info
        uriBuilder.Fragment = "venue-info";
        _driver.Navigate().GoToUrl(uriBuilder.ToString());
        Assert.IsTrue(navLinks[1].GetAttribute("class")!.EndsWith("active"));

        // Act 3: Go to event info
        uriBuilder.Fragment = "event-info";
        _driver.Navigate().GoToUrl(uriBuilder.ToString());
        Assert.IsTrue(navLinks[2].GetAttribute("class")!.EndsWith("active"));

        // Act 4: Go to reserve a seat
        uriBuilder.Fragment = "reserve-a-seat";
        _driver.Navigate().GoToUrl(uriBuilder.ToString());
        Assert.IsTrue(navLinks[3].GetAttribute("class")!.EndsWith("active"));
    }
}

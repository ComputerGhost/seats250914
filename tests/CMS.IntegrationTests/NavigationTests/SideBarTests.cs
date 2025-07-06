using OpenQA.Selenium;

namespace CMS.IntegrationTests.NavigationTests;

[LocalOnly]
[TestClass]
public class SideBarTests
{
    private SeleniumWrapper _driver = null!;

    private IWebElement Sidebar => _driver.FindElement(By.ClassName("app-sidebar"));

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper();
        _driver.SignIn();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void Sidebar_WhenDesktop_IsVertical()
    {
        // Arrange
        const int WINDOW_PADDING_Y = 145;

        // Act
        _driver.ResizeToDesktop();

        // Assert
        Assert.AreEqual(SeleniumWrapper.DESKTOP_HEIGHT, Sidebar.Size.Height, WINDOW_PADDING_Y);
    }

    [TestMethod]
    public void Sidebar_WhenMobile_IsHorizontal()
    {
        // Act
        _driver.ResizeToMobile();

        // Assert
        Assert.AreEqual(81, Sidebar.Size.Height);
    }
}

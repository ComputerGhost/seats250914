using CMS.SmokeTests.Utilities;
using OpenQA.Selenium;

namespace CMS.SmokeTests.Tests;

[TestClass]
[Ignore("These tests will only work on production.")]
public class HostingTests : TestBase
{
    private static IWebDriver Driver { get; set; } = null!;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        Driver = CreateDriver("en");
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        Driver.Quit();
        Driver.Dispose();
    }

    [TestMethod]
    public void CmsSubdomain_RendersCMSWebsite()
    {
        // Arrange
        var cmsSubdomain = new MyUriBuilder(TargetUrl)
            .WithSubdomain("cms")
            .ToString();

        // Act
        Driver.Navigate().GoToUrl(cmsSubdomain);

        // Assert
        Assert.AreEqual("혜린 팬미팅 2025 CMS", Driver.Title);
    }
}

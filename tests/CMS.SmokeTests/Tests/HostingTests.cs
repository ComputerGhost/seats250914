using CMS.SmokeTests.Utilities;
using OpenQA.Selenium;

namespace CMS.SmokeTests.Tests;

[TestClass]
[SmokeTest("These tests will only work on production.")]
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
        // Precondition
        var targetUrlHost = new MyUriBuilder(TargetUrl).Host;
        Assert.AreEqual("cms.", targetUrlHost[..4]);

        // Arrange

        // Act
        Driver.Navigate().GoToUrl(TargetUrl);

        // Assert
        Assert.AreEqual("혜린 팬미팅 2025 관리자 페이지", Driver.Title);
    }
}

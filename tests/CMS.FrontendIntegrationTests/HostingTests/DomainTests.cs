namespace CMS.FrontendIntegrationTests.HostingTests;

[LocalOnly(Mode = ConditionMode.Exclude)]
[TestClass]
public class DomainTests
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
    public void CmsSubdomain_RendersCmsWebsite()
    {
        // Arrange
        var targetUrl = ConfigurationAccessor.Instance.TargetUrl;
        Assert.AreEqual("cms.", new UriBuilder(targetUrl).Host[..4]); // Precondition

        // Act
        _driver.Navigate().GoToUrl(targetUrl);

        // Assert
        Assert.AreEqual("혜린 팬미팅 2025 관리자 페이지", _driver.Title);
    }
}

using OpenQA.Selenium;
using Public.SmokeTests.Utilities;
using System.Net;
using System.Threading.Tasks;

namespace Public.SmokeTests.Tests;

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
    public async Task WwwSubdomain_RedirectsToNoSubdomain()
    {
        // Arrange
        var noSubdomain = TargetUrl;
        var wwwSubdomain = new MyUriBuilder(TargetUrl)
            .WithSubdomain("www")
            .ToString();

        // Act
        using var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        httpClientHandler.AllowAutoRedirect = false;
        using var client = new HttpClient(httpClientHandler);
        var result = await client.GetAsync(wwwSubdomain);

        // Assert
        Assert.AreEqual(HttpStatusCode.MovedPermanently, result.StatusCode);
        Assert.AreEqual(noSubdomain, result.Headers.Location?.ToString().TrimEnd('/'));
    }

    [TestMethod]
    public void NoSubdomain_RendersPublicWebsite()
    {
        // Arrange
        var noSubdomain = TargetUrl;

        // Act
        Driver.Navigate().GoToUrl(noSubdomain);

        // Assert
        Assert.AreEqual("Hyelin Fanmeeting 2025", Driver.Title);
    }

    [TestMethod]
    public async Task Http_InvalidSubdomain_DoesNotRenderContent()
    {
        // Arrange
        var invalidSubdomain = new MyUriBuilder(TargetUrl)
            .WithScheme("http")
            .WithSubdomain("invalid")
            .ToString();

        // Act
        using var client = new HttpClient();
        var action = async () => await client.GetAsync(invalidSubdomain);

        // Assert
        await Assert.ThrowsAsync<HttpRequestException>(action);
    }

    [TestMethod]
    public async Task Https_InvalidSubdomain_DoesNotRenderContent()
    {
        // Arrange
        var invalidSubdomain = new MyUriBuilder(TargetUrl)
            .WithScheme("https")
            .WithSubdomain("invalid")
            .ToString();

        // Act
        using var httpClientHandler = new HttpClientHandler();
        httpClientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        using var client = new HttpClient(httpClientHandler);
        var action = () => client.GetAsync(invalidSubdomain);

        // Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(action);
        Assert.IsNotNull(exception.InnerException);
    }
}

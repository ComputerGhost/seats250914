using System;
using System.Net;

namespace Public.FrontendIntegrationTests.HostingTests;

[LocalOnly(Mode = ConditionMode.Exclude)]
[TestClass]
public class DomainTests
{
    private SeleniumWrapper _driver = null!;

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper(languageId: "en");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public async Task WwwSubdomain_RedirectsToNoSubdomain()
    {
        // Arrange
        var noSubdomain = ConfigurationAccessor.Instance.TargetUrl;
        var uriBuilder = new UriBuilder(noSubdomain);
        uriBuilder.Host = "www." + uriBuilder.Host;
        var wwwSubdomain = uriBuilder.ToString();

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
        // Act
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);

        // Assert
        Assert.AreEqual("Hyelin Fanmeeting 2025", _driver.Title);
    }

    [TestMethod]
    public async Task Http_InvalidSubdomain_DoesNotRenderContent()
    {
        // Arrange
        var uriBuilder = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl);
        uriBuilder.Host = "invalid." + uriBuilder.Host;
        uriBuilder.Port = 80;
        uriBuilder.Scheme = "http";
        var invalidSubdomain = uriBuilder.ToString();

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
        var uriBuilder = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl);
        uriBuilder.Host = "invalid." + uriBuilder.Host;
        uriBuilder.Port = 443;
        uriBuilder.Scheme = "https";
        var invalidSubdomain = uriBuilder.ToString();

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

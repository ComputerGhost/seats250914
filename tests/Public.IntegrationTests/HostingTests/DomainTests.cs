using System;
using System.Net;

namespace Public.IntegrationTests.HostingTests;

[LocalOnly(Mode = ConditionMode.Exclude)]
[TestClass]
public class DomainTests
{
    private SeleniumWrapper _driver = null!;

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
        var response = await client.GetAsync(wwwSubdomain);

        // Assert
        Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
        Assert.AreEqual(noSubdomain, response.Headers.Location?.ToString().TrimEnd('/'));
    }

    [TestMethod]
    public void NoSubdomain_RendersPublicWebsite()
    {
        // Arrange
        using var driver = new SeleniumWrapper(languageId: "en");

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
        var response = await client.GetAsync(invalidSubdomain);

        // Assert
        Assert.IsFalse(response.IsSuccessStatusCode);
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
        var response = await client.GetAsync(invalidSubdomain);

        // Assert
        Assert.IsFalse(response.IsSuccessStatusCode);
    }
}

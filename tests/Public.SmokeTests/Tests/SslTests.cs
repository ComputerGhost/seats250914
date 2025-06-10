using Public.SmokeTests.Utilities;
using System.Net;

namespace Public.SmokeTests.Tests;

[TestClass]
[SmokeTest("These tests will only work on production.")]
public class SslTests : TestBase
{
    public static IEnumerable<string[]> GetInsecureUrls =>
        [
            [new MyUriBuilder(TargetUrl).WithScheme("http").ToString()],
            [new MyUriBuilder(TargetUrl).WithScheme("http").WithSubdomain("www").ToString()],
        ];

    private static IEnumerable<string[]> GetSecureUrls =>
        [
            [TargetUrl],
            [new MyUriBuilder(TargetUrl).WithSubdomain("www").ToString()],
        ];

    [TestMethod]
    [DynamicData(nameof(GetSecureUrls))]
    public async Task Https_IsDeliveredOverSecureConnection(string targetUrl)
    {
        // Arrange

        // Act
        using var client = new HttpClient();
        var response = await client.GetAsync(targetUrl);

        // Assert
        Assert.AreEqual("https", new UriBuilder(targetUrl).Scheme);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    [DynamicData(nameof(GetInsecureUrls))]
    public async Task Http_IsConvertedToHttps(string targetUrl)
    {
        // Arrange

        // Act
        using var httpClientHandler = new HttpClientHandler();
        httpClientHandler.AllowAutoRedirect = false;
        using var client = new HttpClient(httpClientHandler);
        var response = await client.GetAsync(targetUrl);

        // Assert
        Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
        var redirectTo = new UriBuilder(response.Headers.Location!);
        Assert.AreEqual("https", redirectTo.Scheme);
    }

    [TestMethod]
    public async Task Http_WwwSubdomain_RedirectsToHttpsNoSubdomain()
    {
        // Arrange
        var wwwSubdomain = new MyUriBuilder(TargetUrl)
            .WithScheme("http")
            .WithSubdomain("www")
            .ToString();

        // Act
        using var client = new HttpClient();
        var response = await client.GetAsync(wwwSubdomain);

        // Assert
        var redirectTo = response.RequestMessage?.RequestUri;
        var expectedUrl = new MyUriBuilder(TargetUrl);
        Assert.AreEqual(expectedUrl.Scheme, redirectTo?.Scheme);
        Assert.AreEqual(expectedUrl.Host, redirectTo?.Host);
    }
}

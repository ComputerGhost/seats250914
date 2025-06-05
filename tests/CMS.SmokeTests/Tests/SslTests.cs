using CMS.SmokeTests.Utilities;
using System.Net;

namespace CMS.SmokeTests.Tests;

[TestClass]
[Ignore("These tests will only work on production.")]
public class SslTests : TestBase
{
    [TestMethod]
    public async Task Https_IsDeliveredOverSecureConnection()
    {
        // Arrange

        // Act
        using var client = new HttpClient();
        var response = await client.GetAsync(TargetUrl);

        // Assert
        Assert.AreEqual("https", new UriBuilder(TargetUrl).Scheme);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    public async Task Http_IsConvertedToHttps()
    {
        // Arrange
        var insecureUrl = new MyUriBuilder(TargetUrl)
            .WithScheme("http")
            .ToString();

        // Act
        using var httpClientHandler = new HttpClientHandler();
        httpClientHandler.AllowAutoRedirect = false;
        using var client = new HttpClient(httpClientHandler);
        var response = await client.GetAsync(insecureUrl);

        // Assert
        Assert.AreEqual(HttpStatusCode.MovedPermanently, response.StatusCode);
        var redirectTo = new UriBuilder(response.Headers.Location!);
        Assert.AreEqual("https", redirectTo.Scheme);
    }
}

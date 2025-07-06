using System;
using System.Net;

namespace Public.IntegrationTests.HostingTests;

[LocalOnly(Mode = ConditionMode.Exclude)]
[TestClass]
public class SslTests
{
    public static IEnumerable<string[]> InsecureUrlsToTest
    {
        get
        {
            var targetUrl = ConfigurationAccessor.Instance.TargetUrl;
            var uriBuilder = new UriBuilder(targetUrl);
            uriBuilder.Scheme = "http";
            uriBuilder.Port = 80;

            yield return [uriBuilder.ToString()];

            uriBuilder.Host = "www." + uriBuilder.Host;
            yield return [uriBuilder.ToString()];
        }
    }

    private static IEnumerable<string[]> SecureUrlsToTest
    {
        get
        {
            var targetUrl = ConfigurationAccessor.Instance.TargetUrl;
            yield return [targetUrl];

            var uriBuilder = new UriBuilder(targetUrl);
            uriBuilder.Host = "www." + uriBuilder.Host;
            yield return [uriBuilder.ToString()];
        }
    }

    [TestMethod]
    [DynamicData(nameof(InsecureUrlsToTest))]
    public async Task Http_IsConvertedToHttps(string targetUrl)
    {
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
    [DynamicData(nameof(SecureUrlsToTest))]
    public async Task Https_IsDeliveredOverSecureConnection(string targetUrl)
    {
        // Act
        using var client = new HttpClient();
        var response = await client.GetAsync(targetUrl);

        // Assert
        Assert.AreEqual("https", response.RequestMessage?.RequestUri?.Scheme);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }


    [TestMethod]
    public async Task Http_WwwSubdomain_RedirectsToHttpsNoSubdomain()
    {
        // Arrange
        var targetUrl = ConfigurationAccessor.Instance.TargetUrl;
        var uriBuilder = new UriBuilder(targetUrl);
        uriBuilder.Host = "www." + uriBuilder.Host;
        uriBuilder.Port = 870;
        uriBuilder.Scheme = "http";
        var wwwSubdomain = uriBuilder.ToString();

        // Act
        using var client = new HttpClient();
        var response = await client.GetAsync(wwwSubdomain);

        // Assert
        var redirectTo = response.RequestMessage?.RequestUri;
        var expectedUrl = new UriBuilder(targetUrl);
        Assert.AreEqual(expectedUrl.Scheme, redirectTo?.Scheme);
        Assert.AreEqual(expectedUrl.Host, redirectTo?.Host);
    }
}

using System.Net;

namespace CMS.IntegrationTests.HostingTests;

[LocalOnly(Mode = ConditionMode.Exclude)]
[TestClass]
public class SslTests
{
    [TestMethod]
    public async Task Https_IsDeliveredOverSecureConnection()
    {
        // Arrange
        var signInUrl = ConfigurationAccessor.Instance.TargetUrl + "/auth/sign-in";
        Assert.AreEqual("https", new UriBuilder(signInUrl).Scheme); // Precondition

        // Act
        using var client = new HttpClient();
        var response = await client.GetAsync(signInUrl);

        // Assert
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.AreEqual("https", response.RequestMessage?.RequestUri?.Scheme);
    }

    [TestMethod]
    public async Task Http_IsConvertedToHttps()
    {
        // Arrange
        var uriBuilder = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl);
        uriBuilder.Scheme = "http";
        uriBuilder.Port = 80;
        var insecureUrl = uriBuilder.ToString();

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

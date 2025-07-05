using Core.Application.Reservations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CMS.FrontendIntegrationTests.AuthTests;

[TestClass]
public class AuthorizationTests
{
    private const string SIGN_IN_PATH = "/auth/sign-in";

    private SeleniumWrapper? _driver;
    private SeleniumWrapper Driver => _driver ??= new SeleniumWrapper();

    [TestCleanup]
    public void Cleanup()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }

    [TestMethod]
    public async Task SetUp_WhenAccessed_Fails()
    {
        // Arrange
        const string SETUP_PATH = "/auth/setup";
        var targetUrl = ConfigurationAccessor.Instance.TargetUrl + SETUP_PATH;

        // Act
        using var httpClientHandler = new HttpClientHandler();
        httpClientHandler.AllowAutoRedirect = false;
        using var client = new HttpClient(httpClientHandler);
        var response = await client.GetAsync(targetUrl);

        // Assert
        Assert.AreEqual(HttpStatusCode.Locked, response.StatusCode);
    }

    [TestMethod]
    public async Task SignOut_WhenAccessed_Fails()
    {
        // Arrange
        var targetUrl = ConfigurationAccessor.Instance.TargetUrl + "/auth/sign-out";

        // Act
        using var httpClientHandler = new HttpClientHandler();
        httpClientHandler.AllowAutoRedirect = false;
        using var client = new HttpClient(httpClientHandler);
        var response = await client.GetAsync(targetUrl);

        // Assert
        Assert.AreEqual(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }

    [LocalOnly]
    [DataTestMethod]
    [DataRow("/")]
    [DataRow("/accounts/")]
    [DataRow("/accounts/new")]
    [DataRow("/accounts/{login}/details")]
    [DataRow("/accounts/{login}/edit")]
    [DataRow("/configuration")]
    [DataRow("/reservations/")]
    [DataRow("/reservations/new")]
    [DataRow("/reservations/{reservationId}/details")]
    [DataRow("/reservations/{reservationId}/edit")]
    public async Task RestrictedUrl_WhenAccessed_RedirectsToSignIn(string targetPattern)
    {
        // Arrange
        var targetUrl = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl);
        targetUrl.Path = await ResolvePattern(targetPattern);

        // Act 1: Go to page
        Driver.Navigate().GoToUrl(targetUrl.Uri);

        // Act 2: Wait for page to load
        Driver.WaitUntil(d => new UriBuilder(d.Url).Path == SIGN_IN_PATH);

        // Assert
        Assert.AreEqual(SIGN_IN_PATH, new UriBuilder(Driver.Url).Path);
    }

    private static async Task<string> ResolvePattern(string pattern)
    {
        if (pattern.Contains("{login}"))
        {
            return pattern.Replace("{login}", ConfigurationAccessor.Instance.Username);
        }

        if (pattern.Contains("{reservationId}"))
        {
            // Delete all reservations and create a new one to use.
            var mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;
            await TestDataSetup.DeleteTestAccount();
            var reservationId = (await mediator.Send(new AdminReserveSeatCommand
            {
                SeatNumber = 1,
                Name = "bob",
                Email = "bob@example.com",
                PreferredLanguage = "English",
            })).Value;
            return pattern.Replace("{reservationId}", reservationId.ToString());
        }

        return pattern;
    }
}

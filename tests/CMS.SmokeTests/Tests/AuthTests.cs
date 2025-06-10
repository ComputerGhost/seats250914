using CMS.SmokeTests.Utilities;
using OpenQA.Selenium;
using System.Web;

namespace CMS.SmokeTests.Tests;

[TestClass]
[SmokeTest("These tests will only work on a development instance.")]
public class AuthTests : TestBase
{
    private const string CONFIGURATION_PATH = "/configuration";
    private const string DASHBOARD_PATH = "/";
    private const string SIGN_IN_PATH = "/auth/sign-in";

    private const string USERNAME = "initial";
    private const string PASSWORD = "initial";

    private IWebDriver Driver { get; set; } = null!;

    [TestInitialize]
    public void Initialize()
    {
        Driver = CreateDriver("en");
    }

    [TestCleanup]
    public void Cleanup()
    {
        Driver.Quit();
        Driver.Dispose();
    }

    [TestMethod]
    public async Task Dashboard_WhenNotSignedIn_RedirectsToSignIn()
    {
        // Arrange
        var dashboardUrl = $"{TargetUrl}{DASHBOARD_PATH}";

        // Act
        Driver.Navigate().GoToUrl(dashboardUrl);
        await WaitForPageLoad();

        // Assert
        var parsedCurrentUrl = new UriBuilder(Driver.Url);
        Assert.AreEqual(SIGN_IN_PATH, parsedCurrentUrl.Path);
    }

    [TestMethod]
    public async Task SignIn_WhenSuccessful_AndReturnUrlIsOmitted_RedirectsToDashboard()
    {
        // Arrange
        var signInUrl = $"{TargetUrl}{SIGN_IN_PATH}";
        Driver.Navigate().GoToUrl(signInUrl);

        // Act
        Driver.FindElement(By.Id("Login")).SendKeys(USERNAME);
        Driver.FindElement(By.Id("Password")).SendKeys(PASSWORD);
        Driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);
        await WaitForPageLoad();

        // Assert
        var parsedCurrentUrl = new UriBuilder(Driver.Url);
        Assert.AreEqual(DASHBOARD_PATH, parsedCurrentUrl.Path);
    }

    [TestMethod]
    public async Task SignIn_WhenSuccessful_AndReturnUrlIsAbsolute_RedirectsToDashboard()
    {
        // Arrange
        var encodedReturnUrl = HttpUtility.UrlEncode("https://www.google.com");
        var signInUrl = $"{TargetUrl}{SIGN_IN_PATH}?returnUrl={encodedReturnUrl}";
        Driver.Navigate().GoToUrl(signInUrl);

        // Act
        Driver.FindElement(By.Id("Login")).SendKeys(USERNAME);
        Driver.FindElement(By.Id("Password")).SendKeys(PASSWORD);
        Driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);
        await WaitForPageLoad();

        // Assert
        var parsedCurrentUrl = new UriBuilder(Driver.Url);
        Assert.AreEqual(DASHBOARD_PATH, parsedCurrentUrl.Path);
    }

    [TestMethod]
    // I'm using a ReturnUrl of "/configuration" to mix it up.
    public async Task SignIn_WhenSuccessful_AndReturnUrlIsConfiguration_RedirectsToConfiguration()
    {
        // Arrange
        var encodedReturnUrl = HttpUtility.UrlEncode(CONFIGURATION_PATH);
        var signInUrl = $"{TargetUrl}{SIGN_IN_PATH}?returnUrl={encodedReturnUrl}";
        Driver.Navigate().GoToUrl(signInUrl);

        // Act
        Driver.FindElement(By.Id("Login")).SendKeys(USERNAME);
        Driver.FindElement(By.Id("Password")).SendKeys(PASSWORD);
        Driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);
        await WaitForPageLoad();

        // Assert
        var parsedCurrentUrl = new UriBuilder(Driver.Url);
        Assert.AreEqual(CONFIGURATION_PATH, parsedCurrentUrl.Path);
    }

    [TestMethod]
    public async Task SignIn_WhenWrongPassword_DoesNotSignIn()
    {
        // Arrange
        var signInUrl = $"{TargetUrl}{SIGN_IN_PATH}";
        Driver.Navigate().GoToUrl(signInUrl);

        // Act
        Driver.FindElement(By.Id("Login")).SendKeys(USERNAME);
        Driver.FindElement(By.Id("Password")).SendKeys("invalid");
        Driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);
        await WaitForPageLoad();

        // Assert
        var parsedCurrentUrl = new UriBuilder(Driver.Url);
        Assert.AreEqual(SIGN_IN_PATH, parsedCurrentUrl.Path);
        Assert.IsTrue(Driver.FindElements(By.Id("error-display")).Any());
    }

    [TestMethod]
    public async Task SignIn_WhenWrongUsername_DoesNotSignIn()
    {
        // Arrange
        var signInUrl = $"{TargetUrl}{SIGN_IN_PATH}";
        Driver.Navigate().GoToUrl(signInUrl);

        // Act
        Driver.FindElement(By.Id("Login")).SendKeys("invalid");
        Driver.FindElement(By.Id("Password")).SendKeys(PASSWORD);
        Driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);
        await WaitForPageLoad();

        // Assert
        var parsedCurrentUrl = new UriBuilder(Driver.Url);
        Assert.AreEqual(SIGN_IN_PATH, parsedCurrentUrl.Path);
        Assert.IsTrue(Driver.FindElements(By.Id("error-display")).Any());
    }
}

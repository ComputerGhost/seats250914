using Core.Application.Accounts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;
using System.Web;

namespace CMS.IntegrationTests.AuthTests;

[TestClass]
public class AuthenticationTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private static string ConfigurationUrl => ConfigurationAccessor.Instance.TargetUrl + "/configuration";
    private static string DashboardUrl => ConfigurationAccessor.Instance.TargetUrl + "/";
    private static string SignInUrl => ConfigurationAccessor.Instance.TargetUrl + "/auth/sign-in";

    private static string Username => ConfigurationAccessor.Instance.Username;
    private static string Password => ConfigurationAccessor.Instance.Password;

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper();
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public void Dashboard_WhenNotSignedIn_RedirectsToSignIn()
    {
        // Arrange
        var signInPath = new UriBuilder(SignInUrl).Path;

        // Act: Load the dashboard
        _driver.Navigate().GoToUrl(DashboardUrl);

        // Assert
        Assert.AreEqual(signInPath, new UriBuilder(_driver.Url).Path);
    }

    [LocalOnly]
    [TestMethod]
    public void SignIn_WhenSuccessful_AndReturnUrlIsOmitted_RedirectsToDashbard()
    {
        // Arrange
        var dashboardPath = new UriBuilder(DashboardUrl).Path;

        // Act 1: Load the page and sign in.
        _driver.Navigate().GoToUrl(SignInUrl);
        _driver.FindElement(By.Id("Login")).SendKeys(Username);
        _driver.FindElement(By.Id("Password")).SendKeys(Password);
        _driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Act 2: Wait for the page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == dashboardPath);

        // Assert
        Assert.AreEqual(dashboardPath, new UriBuilder(_driver.Url).Path);
    }

    [LocalOnly]
    [TestMethod]
    public void SignIn_WhenSuccessful_AndReturnUrlIsAbsolute_RedirectsToDashbard()
    {
        // Arrange
        var dashboardPath = new UriBuilder(DashboardUrl).Path;
        var encodedReturnUrl = HttpUtility.UrlEncode("https://www.google.com");
        var signInUrl = $"{SignInUrl}?returnUrl={encodedReturnUrl}";

        // Act 1: Load the page and sign in.
        _driver.Navigate().GoToUrl(signInUrl);
        _driver.FindElement(By.Id("Login")).SendKeys(Username);
        _driver.FindElement(By.Id("Password")).SendKeys(Password);
        _driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Act 2: Wait for the page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == dashboardPath);

        // Assert
        Assert.AreEqual(dashboardPath, new UriBuilder(_driver.Url).Path);
    }

    [LocalOnly]
    [TestMethod]
    public void SignIn_WhenSuccessful_AndReturnUrlIsRelative_RedirectsToReturnUrl()
    {
        // Arrange
        var targetPage = ConfigurationUrl;
        var targetPath = new UriBuilder(targetPage).Path;

        // Act 1: Visit target page to let it redirect to sign in with the return url.
        _driver.Navigate().GoToUrl(targetPage);

        // Act 2: Sign in
        _driver.FindElement(By.Id("Login")).SendKeys(Username);
        _driver.FindElement(By.Id("Password")).SendKeys(Password);
        _driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Act 3: Wait for the page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == targetPath);

        // Assert
        Assert.AreEqual(targetPath, new UriBuilder(_driver.Url).Path);
    }

    [TestMethod]
    public void SignIn_WhenWrongPassword_DoesNotSignIn()
    {
        // Arrange
        var signInPath = new UriBuilder(SignInUrl).Path;

        // Act 1: Load the page and use the wrong credentials
        _driver.Navigate().GoToUrl(SignInUrl);
        _driver.FindElement(By.Id("Login")).SendKeys(Username);
        _driver.FindElement(By.Id("Password")).SendKeys(Password + "-not");
        _driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Act 2: Wait for the page to load
        _driver.WaitUntil(d => d.FindElements(By.Id("error-display")).Any());

        // Assert
        Assert.AreEqual(signInPath, new UriBuilder(_driver.Url).Path);
        Assert.IsTrue(_driver.FindElements(By.Id("error-display")).Any());
    }

    [TestMethod]
    public void SignIn_WhenWrongUsername_DoesNotSignIn()
    {
        // Arrange
        var signInPath = new UriBuilder(SignInUrl).Path;

        // Act 1: Load the page and use the wrong credentials
        _driver.Navigate().GoToUrl(SignInUrl);
        _driver.FindElement(By.Id("Login")).SendKeys(Username + "-not");
        _driver.FindElement(By.Id("Password")).SendKeys(Password);
        _driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Act 2: Wait for the page to load
        _driver.WaitUntil(d => d.FindElements(By.Id("error-display")).Any());

        // Assert
        Assert.AreEqual(signInPath, new UriBuilder(_driver.Url).Path);
        Assert.IsTrue(_driver.FindElements(By.Id("error-display")).Any());
    }

    [TestMethod]
    public async Task SignIn_WhenUserInactive_DoesNotSignIn()
    {
        // Arrange
        var signInPath = new UriBuilder(SignInUrl).Path;
        var disabledLogin = await TestDataSetup.CreateTestAccount(enabled: false);
        var password = TestDataSetup.GenerateSecurePassword();
        await _mediator.Send(new UpdatePasswordCommand
        {
            Login = disabledLogin,
            Password = password,
        });

        // Act 1: Load the page and sign in
        _driver.Navigate().GoToUrl(SignInUrl);
        _driver.FindElement(By.Id("Login")).SendKeys(disabledLogin);
        _driver.FindElement(By.Id("Password")).SendKeys(password);
        _driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Act 2: Wait for the page to load
        _driver.WaitUntil(d => d.FindElements(By.Id("error-display")).Any());

        // Assert
        Assert.AreEqual(signInPath, new UriBuilder(_driver.Url).Path);
        Assert.IsTrue(_driver.FindElements(By.Id("error-display")).Any());
    }

    [LocalOnly]
    [TestMethod]
    public void SignOut_WhenClicked_SignsOutUser()
    {
        // Arrange
        var signInPath = new UriBuilder(SignInUrl).Path;
        _driver.SignIn();

        // Act 1: Click to sign out
        var userMenu = _driver.FindElement(By.ClassName("user-menu"));
        var signOutButton = userMenu.FindElement(By.TagName("button"));
        userMenu.Click();
        signOutButton.Click();

        // Assert we are on the sign in page
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path != "/");
        Assert.AreEqual(signInPath, new UriBuilder(_driver.Url).Path);

        // Act 2: Try to load dashboard
        _driver.Navigate().GoToUrl(DashboardUrl);

        // Assert we are on the sign in page
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path != "/");
        Assert.AreEqual(signInPath, new UriBuilder(_driver.Url).Path);
    }
}

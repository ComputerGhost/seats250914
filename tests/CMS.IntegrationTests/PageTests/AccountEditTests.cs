using Core.Application.Accounts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenQA.Selenium;

namespace CMS.IntegrationTests.PageTests;

[LocalOnly]
[TestClass]
public class AccountEditTests
{
    private SeleniumWrapper _driver = null!;
    private IMediator _mediator = null!;

    private IWebElement EnabledCheck => _driver.FindElement(By.Id("IsEnabled"));
    private IWebElement PasswordText => _driver.FindElement(By.Id("Password"));
    private IWebElement UpdatePasswordButton => _driver.FindElement(By.ClassName("btn-secondary"));
    private IWebElement UpdateUserButton => _driver.FindElement(By.ClassName("btn-primary"));

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper();
        _mediator = ConfigurationAccessor.Instance.Services.GetService<IMediator>()!;

        _driver.SignIn();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public async Task UserForm_WhenSubmitted_RedirectsToDetails()
    {
        // Arrange
        var login = await TestDataSetup.CreateTestAccount();
        var editUrl = ConfigurationAccessor.Instance.TargetUrl + $"/accounts/{login}/edit";
        var detailsPath = $"/accounts/{login}/details";

        // Act 1: Load page and submit form
        _driver.Navigate().GoToUrl(editUrl);
        UpdateUserButton.Click();

        // Act 2: Wait for new page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == detailsPath);

        // Assert
        Assert.AreEqual(detailsPath, new UriBuilder(_driver.Url).Path);
    }

    [TestMethod]
    public async Task UserForm_WhenSubmitted_UpdatesUser()
    {
        // Arrange
        var login = await TestDataSetup.CreateTestAccount();
        var editUrl = ConfigurationAccessor.Instance.TargetUrl + $"/accounts/{login}/edit";

        // Act 1: Navigate to page
        _driver.Navigate().GoToUrl(editUrl);

        // Act 2: Disable user
        EnabledCheck.Click();
        UpdateUserButton.Click();

        // Assert: Check if disabled
        var userInfo = await _mediator.Send(new FetchAccountQuery(login));
        Assert.IsFalse(userInfo.Value.IsEnabled);

        // Act 3: Go to another page (so we can detect navigation), then return.
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl);
        _driver.Navigate().GoToUrl(editUrl);

        // Act 3: Enable user
        EnabledCheck.Click();
        UpdateUserButton.Click();

        // Assert: Check if enabled
        userInfo = await _mediator.Send(new FetchAccountQuery(login));
        Assert.IsTrue(userInfo.Value.IsEnabled);
    }

    [TestMethod]
    public async Task PasswordForm_WhenSubmitted_UpdatesPassword()
    {
        // Arrange
        var login = await TestDataSetup.CreateTestAccount();
        var dashboardPath = new UriBuilder(ConfigurationAccessor.Instance.TargetUrl).Path;
        var editUrl = ConfigurationAccessor.Instance.TargetUrl + $"/accounts/{login}/edit";
        var password = TestDataSetup.GenerateSecurePassword();

        // Act 1: Navigate to page
        _driver.Navigate().GoToUrl(editUrl);

        // Act 2: Change password
        PasswordText.SendKeys(password);
        UpdatePasswordButton.Click();

        // Act 3: Sign out
        _driver.Manage().Cookies.DeleteAllCookies();

        // Act 4: Sign in as test user
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "/auth/sign-in");
        _driver.FindElement(By.Id("Login")).SendKeys(login);
        _driver.FindElement(By.Id("Password")).SendKeys(password);
        _driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Act 5: Wait for page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == dashboardPath);

        // Assert
        Assert.AreEqual(dashboardPath, new UriBuilder(_driver.Url).Path);

    }
}

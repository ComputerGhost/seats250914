using OpenQA.Selenium;

namespace CMS.IntegrationTests.PageTests;

[LocalOnly]
[TestClass]
public class AccountCreateTest
{
    private SeleniumWrapper _driver = null!;

    private IWebElement Login => _driver.FindElement(By.Id("Login"));
    private IWebElement Password => _driver.FindElement(By.Id("Password"));
    private IWebElement Submit => _driver.FindElement(By.ClassName("btn-primary"));

    [TestInitialize]
    public void Initialize()
    {
        _driver = new SeleniumWrapper();

        _driver.SignIn();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver.Quit();
        _driver.Dispose();
    }

    [TestMethod]
    public async Task UserForm_WhenSubmitted_CreatesUser_AndRedirectsToDetails()
    {
        // Arrange
        await TestDataSetup.DeleteTestAccount();
        var login = TestDataSetup.TEST_USER_LOGIN;
        var password = TestDataSetup.GenerateSecurePassword();
        var detailsPath = $"/accounts/{login}/details";

        // Act 1: Load page and submit form
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "/accounts/new");
        Login.SendKeys(login);
        Password.SendKeys(password);
        Submit.Click();

        // Act 2: Verify details load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == detailsPath);
        Assert.AreEqual(detailsPath, new UriBuilder(_driver.Url).Path);

        // Act 3: Sign out
        _driver.Manage().Cookies.DeleteAllCookies();

        // Act 4: Sign in as new user
        _driver.Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "/auth/sign-in");
        _driver.FindElement(By.Id("Login")).SendKeys(login);
        _driver.FindElement(By.Id("Password")).SendKeys(password);
        _driver.FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Act 5: Wait for page to load
        _driver.WaitUntil(d => new UriBuilder(d.Url).Path == "/");

        // Assert
        Assert.AreEqual("/", new UriBuilder(_driver.Url).Path);
    }
}

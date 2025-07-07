using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Drawing;

namespace CMS.IntegrationTests;
internal class SeleniumWrapper : ChromeDriver
{
    public const int DESKTOP_WIDTH = 1024;
    public const int DESKTOP_HEIGHT = 768;
    public const int MOBILE_WIDTH = 576;
    public const int MOBILE_HEIGHT = 576;

    public SeleniumWrapper(string languageId = "en")
        : base(BuildOptions(languageId))
    {
    }

    private static ChromeOptions BuildOptions(string languageId)
    {
        var options = new ChromeOptions();
        options.AddArgument($"--accept-lang={languageId}");
        options.AddArgument("--incognito");
        return options;
    }

    public void Refresh()
    {
        var currentUrl = Url;
        var currentPath = new UriBuilder(currentUrl).Path;
        var replacementUrl = ConfigurationAccessor.Instance.TargetUrl + "/loading";
        var replacementPath = "/loading";

        // Load another page.
        Navigate().GoToUrl(replacementUrl);
        WaitUntil(d => new UriBuilder(d.Url).Path == replacementPath);

        // Go back to the original page.
        Navigate().GoToUrl(currentUrl);
        WaitUntil(d => new UriBuilder(d.Url).Path == currentPath);
    }

    public void ResizeToDesktop()
    {
        Manage().Window.Size = new Size(DESKTOP_WIDTH, DESKTOP_HEIGHT);
    }

    public void ResizeToMobile()
    {
        Manage().Window.Size = new Size(MOBILE_WIDTH, MOBILE_HEIGHT);
    }

    public void ScrollTo(IWebElement element)
    {
        ExecuteScript("arguments[0].scrollIntoView(true);", element);

        // Take no action for 1 second.
        // Trying to click or doing any polling during this time will stop the scrolling.
        WaitUntil(_ => false, 1);
    }

    /// <summary>
    /// Sets the value of an element directly.
    /// </summary>
    public void SetValue(IWebElement inputElement, string value)
    {
        ExecuteScript($"arguments[0].value = '{value}';", inputElement);
    }

    public void SignIn()
    {
        // Load the sign-in page and sign in.
        Navigate().GoToUrl(ConfigurationAccessor.Instance.TargetUrl + "/auth/sign-in");
        FindElement(By.Id("Login")).SendKeys(ConfigurationAccessor.Instance.Username);
        FindElement(By.Id("Password")).SendKeys(ConfigurationAccessor.Instance.Password);
        FindElement(By.Id("Password")).SendKeys(Keys.Return);

        // Wait for the page to load.
        WaitUntil(d => new UriBuilder(d.Url).Path == "/");
    }

    public bool WaitUntil(Func<IWebDriver, bool> condition, int maxSeconds = 2)
    {
        var wait = new WebDriverWait(this, TimeSpan.FromSeconds(maxSeconds))
        {
            PollingInterval = TimeSpan.FromMilliseconds(100),
        };

        try
        {
            wait.Until(condition);
        }
        catch (WebDriverTimeoutException) { }

        return condition(this);
    }
}

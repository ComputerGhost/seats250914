using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Drawing;

namespace CMS.FrontendIntegrationTests;
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

    public void SetDateField(IWebElement inputElement, DateTimeOffset date)
    {
        // We can't just type the keys because the format can differ between computers.
        // So instead we use JS to set it.
        var formattedDate = date.ToString("yyyy-MM-dd");
        ExecuteScript($"arguments[0].value = '{formattedDate}';", inputElement);
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
            PollingInterval = TimeSpan.FromMilliseconds(300),
        };

        try
        {
            wait.Until(condition);
        }
        catch (WebDriverTimeoutException) { }

        return condition(this);
    }
}

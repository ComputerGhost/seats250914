using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Drawing;

namespace Public.FrontendIntegrationTests;
internal class SeleniumWrapper : ChromeDriver
{
    public SeleniumWrapper(string languageId = "en")
        : base(BuildOptions(languageId))
    {
    }

    private static ChromeOptions BuildOptions(string languageId)
    {
        var options = new ChromeOptions();
        options.AddArgument($"--accept-lang={languageId}");
        return options;
    }

    public void ResizeToDesktop()
    {
        Manage().Window.Size = new Size(1024, 768);
    }

    public void ResizeToMobile()
    {
        Manage().Window.Size = new Size(576, 576);
    }

    public void ScrollTo(IWebElement element)
    {
        ExecuteScript("arguments[0].scrollIntoView(true);", element);

        // Take no action for 1 second.
        // Trying to click or doing any polling during this time will stop the scrolling.
        WaitUntil(_ => false, 1);
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

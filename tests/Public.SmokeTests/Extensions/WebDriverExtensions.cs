using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Public.SmokeTests.Extensions;
internal static class WebDriverExtensions
{
    public static void ScrollTo(this IWebDriver driver, IWebElement element)
    {
        var javaScript = driver as IJavaScriptExecutor
            ?? throw new NotImplementedException("The browser driver does not support JavaScript.");
        javaScript.ExecuteScript("arguments[0].scrollIntoView(true);", element);
    }

    /// <summary>
    /// `IWebElement.Click` should do this automatically, but sometimes it's finicky.
    /// </summary>
    public static void ScrollToAndClick(this IWebDriver driver, IWebElement element)
    {
        driver.ScrollTo(element);

        // Take no action for 1 second.
        // Trying to click or doing any polling during this time will stop the scrolling.
        try
        {
            driver.WaitUntil(_ => false, TimeSpan.FromSeconds(1));
        }
        catch (WebDriverTimeoutException) {}

        element.Click();
    }

    public static void WaitUntil(this IWebDriver driver, Func<IWebDriver, bool> condition, TimeSpan? duration = null)
    {
        duration ??= TimeSpan.FromSeconds(2);
        var wait = new WebDriverWait(driver, duration.Value);
        wait.Until(condition);
    }
}

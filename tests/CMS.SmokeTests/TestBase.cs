using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace CMS.SmokeTests;

[TestClass]
public class TestBase
{
    protected static string TargetUrl { get; private set; } = null!;

    [AssemblyInitialize]
    public static void InitializeAssembly(TestContext context)
    {
        var environment = Environment.GetEnvironmentVariable("TEST_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("testsettings.json")
            .AddJsonFile($"testsettings.{environment}.json", optional: true)
            .Build();
        TargetUrl = configuration["targetUrl"]!;
    }

    protected static IWebDriver CreateDriver(string languageId)
    {
        var options = new ChromeOptions();
        options.AddArgument($"--lang=${languageId}");
        return new ChromeDriver(options);
    }

    protected Task WaitForPageLoad()
    {
        /**
         * I could do this better... this is what I did at work:
         *  1. Check page URL right before last navigate.
         *  2. Wait until the URL changes from that state.
         * 
         * But it's complicated. A simple time delay will suffice for this project.
         */
        return Task.Delay(TimeSpan.FromSeconds(1));
    }
}

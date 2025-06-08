using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Public.SmokeTests.Tests;

public class TestBase
{
    protected static string TargetUrl { get; private set; } = null!;

    static TestBase()
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
        options.AddArgument($"--accept-lang={languageId}");
        return new ChromeDriver(options);
    }
}

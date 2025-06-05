using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Public.SmokeTests;

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
}

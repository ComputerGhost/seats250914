using OpenQA.Selenium.Chrome;
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
}

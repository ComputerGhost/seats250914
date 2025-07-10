using RazorLight;
using Serilog;

namespace EmailSender.Services;
internal class RazorTemplateService
{
    private readonly RazorLightEngine _engine = null!;

    public RazorTemplateService()
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates");
        _engine = new RazorLightEngineBuilder()
            .UseFileSystemProject(templatePath)
            .UseMemoryCachingProvider()
            .Build();
    }

    public Task<string> Render(string templateName, string languageId, object? model)
    {
        var fileName = $"{templateName}.{languageId}.cshtml";
        Log.Debug("Generate html from template file {fileName}.", fileName);
        return _engine.CompileRenderAsync(fileName, model);
    }
}

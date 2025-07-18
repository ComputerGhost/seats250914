using Core.Application;
using Core.Domain.DependencyInjection;
using Presentation.Shared.FrameworkEnhancements.Filters;
using Presentation.Shared.Localization.Extensions;
using Presentation.Shared.LockCleanup;
using Presentation.Shared.Logging.Extensions;
using Public.Hubs;
using Public.Models;
using System.Globalization;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc(options =>
{
    options.Filters.Add<SerilogExceptionLoggingFilter>();
    options.Filters.Add<ValidationExceptionFilter>();
});
builder.Services.AddCleanupScheduler(options => options.MaxWaitSeconds = 60 * 60);
builder.Services.AddCore(options =>
{
    builder.Configuration.Bind("InfrastructureOptions", options);
});
builder.Services.AddMyLogging(options =>
{
    builder.Configuration.Bind("MyLogging", options);
});
builder.Services.AddMyLocalization(options =>
{
    options.SupportedCultures =
    [
        new CultureInfo("en"),
        new CultureInfo("ko"),
        new CultureInfo("zh-Hans"),
    ];
    options.DefaultCulture = new CultureInfo("en");
});
builder.Services.AddServiceImplementations(Assembly.GetExecutingAssembly());
builder.Services.AddSignalR();
builder.Services.Configure<Config>(builder.Configuration.GetSection("Config"));

var app = builder.Build();
app.MapControllers();
app.MapHub<SeatsHub>("/api/watch-seats");
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "immutable,max-age=31536000,public");
    }
});
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseMyLocalization();
app.Run();

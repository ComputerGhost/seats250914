using Core.Application;
using Presentation.Shared.Localization.Extensions;
using Presentation.Shared.LockCleanup;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc();
builder.Services.AddCleanupScheduler(options => options.MaxWaitSeconds = 60 * 60);
builder.Services.AddCore(options =>
{
    builder.Configuration.Bind("InfrastructureOptions", options);
});
builder.Services.AddMyLocalization(options =>
{
    options.SupportedCultures =
    [
        new CultureInfo("en"),
        new CultureInfo("ko"),
    ];
    options.DefaultCulture = new CultureInfo("en");
});

var app = builder.Build();
app.MapControllers();
app.UseStaticFiles();
//app.UseExceptionHandler("/Error");
//app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseMyLocalization();
app.Run();


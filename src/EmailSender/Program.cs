using Core.Application;
using EmailSender;
using EmailSender.Models;
using EmailSender.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Presentation.Shared.Localization.Extensions;
using Presentation.Shared.Logging.Extensions;
using System.Globalization;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var builder = Host.CreateApplicationBuilder();

builder.Configuration
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json")
    .AddEnvironmentVariables();

builder.Services.Configure<EmailOptions>(options => builder.Configuration.Bind("EmailOptions", options));

builder.Services.AddCore(options => builder.Configuration.Bind("InfrastructureOptions", options));
builder.Services.AddMyLogging(options => builder.Configuration.Bind("MyLogging", options));
builder.Services.AddMyLocalization(options =>
{
    options.SupportedCultures =
    [
        new CultureInfo("en"),
        new CultureInfo("ko"),
    ];
    options.DefaultCulture = new CultureInfo("en");
});

builder.Services.AddSingleton<EmailProcessorService>();
builder.Services.AddSingleton<RazorTemplateService>();
builder.Services.AddSingleton<SmtpEmailSender>();

builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.Run();
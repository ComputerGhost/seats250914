using CMS.Features.Authentication;
using Core.Application;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Localization.Extensions;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddCore(options =>
{
    builder.Configuration.Bind("InfrastructureOptions", options);
});
builder.Services.AddMyAuthentication(options =>
{
    builder.Configuration.Bind("AuthenticationOptions", options);
    options.LoginPath = "/auth/sign-in";
});
builder.Services.AddMyLocalization(options =>
{
    options.SupportedCultures = [ new CultureInfo("ko"), ];
    options.DefaultCulture = new CultureInfo("ko");
});

var app = builder.Build();
app.MapControllers();
app.UseStaticFiles();
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseMyLocalization();
app.Run();

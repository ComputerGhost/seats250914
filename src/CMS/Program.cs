using CMS.Features.Authentication;
using Core.Application;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc();
builder.Services.AddMyAuthentication(options =>
{
    options.LoginPath = "/accounts/sign-in";
});
builder.Services.AddCore(options =>
{
    builder.Configuration.Bind("InfrastructureOptions", options);
});

var app = builder.Build();
app.MapControllers();
app.UseStaticFiles();
//app.UseExceptionHandler("/Error");
//app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.Run();

using Core.Application;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc();
builder.Services.AddCore(options =>
{
    builder.Configuration.Bind("InfrastructureOptions", options);
});

var app = builder.Build();
app.MapControllers();
app.UseStaticFiles();

app.Run();

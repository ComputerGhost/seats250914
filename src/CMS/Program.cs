using Core.Application;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMvc();
builder.Services.AddCore(options =>
{
});

var app = builder.Build();
app.MapControllers();
app.UseStaticFiles();

app.Run();

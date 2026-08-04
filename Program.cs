using MiniBillingSystem;

if (args.Length > 0 && (args[0] == "--console" || args[0] == "--cli"))
{
    ConsoleMenu.Run(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

// Fallback to index.html for SPA if client build is served
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
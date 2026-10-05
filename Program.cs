using MiniBillingSystem;

if (args.Length > 0 && (args[0] == "--console" || args[0] == "--cli"))
{
    ConsoleMenu.Run(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Read PORT environment variable for Vercel and container deployments
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

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

// Initialize SQLite database
var connStr = app.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=billing.db;Cache=Shared";
DbInitializer.Initialize(connStr);

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.MapGet("/healthz", () => Results.Ok(new { status = "Healthy", service = "Frontend (Vite/React)" }));

// Fallback to index.html for SPA if client build is served
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
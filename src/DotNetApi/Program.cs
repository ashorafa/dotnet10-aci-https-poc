using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// Configure Kestrel for HTTPS with TLS certificate
builder.WebHost.ConfigureKestrel(options =>
{
    // Read certificate settings from environment
    var certificatePath = Environment.GetEnvironmentVariable("Kestrel:Certificates:Default:Path") 
        ?? "/app/certs/cert.pfx";
    var certificatePassword = Environment.GetEnvironmentVariable("Kestrel:Certificates:Default:Password") 
        ?? "changeme";

    // Configure HTTPS on port 443
    options.ListenAnyIP(443, listenOptions =>
    {
        // Only configure certificate if it exists
        if (File.Exists(certificatePath))
        {
            var certificate = new X509Certificate2(certificatePath, certificatePassword);
            listenOptions.UseHttps(certificate);
            Console.WriteLine($"[Kestrel] Loaded certificate from: {certificatePath}");
        }
        else
        {
            Console.WriteLine($"[Kestrel] Certificate not found at: {certificatePath}. Running without HTTPS.");
        }
    });

    // Also listen on HTTP port 80 for health checks / diagnostics
    options.ListenAnyIP(80);
});

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Startup message
var sampleConfigValue = Environment.GetEnvironmentVariable("SampleConfigValue") ?? "Not configured";
Console.WriteLine($"[Startup] SampleConfigValue: {sampleConfigValue}");
Console.WriteLine($"[Startup] Environment: {app.Environment.EnvironmentName}");
Console.WriteLine("[Startup] .NET 10 API ready to accept requests on HTTPS (port 443) and HTTP (port 80)");

await app.RunAsync();

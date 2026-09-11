using Microsoft.AspNetCore.Mvc;

namespace DotNetApi.Controllers;

/// <summary>
/// Health check endpoint that demonstrates configuration reading
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// GET /health
    /// Returns health status and reads SampleConfigValue from environment
    /// </summary>
    [HttpGet(Name = "GetHealth")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        var sampleConfigValue = Environment.GetEnvironmentVariable("SampleConfigValue") ?? "Not configured";
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
        var timestamp = DateTime.UtcNow;

        var response = new
        {
            status = "healthy",
            sampleConfigValue = sampleConfigValue,
            environment = environment,
            timestamp = timestamp,
            dotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            machineTime = Environment.MachineName
        };

        _logger.LogInformation("Health check called - SampleConfigValue: {ConfigValue}", sampleConfigValue);

        return Ok(response);
    }

    /// <summary>
    /// GET /health/config
    /// Returns all environment-based configuration
    /// </summary>
    [HttpGet("config", Name = "GetConfig")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfig()
    {
        var response = new
        {
            sampleConfigValue = Environment.GetEnvironmentVariable("SampleConfigValue"),
            aspnetcoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            dotnetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            osInfo = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            processor = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture,
            timestamp = DateTime.UtcNow
        };

        return Ok(response);
    }

    /// <summary>
    /// GET /health/ready
    /// Kubernetes-style readiness probe
    /// </summary>
    [HttpGet("ready", Name = "GetReady")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    public IActionResult GetReady()
    {
        return Ok(new { ready = true, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// GET /health/live
    /// Kubernetes-style liveness probe
    /// </summary>
    [HttpGet("live", Name = "GetLive")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLive()
    {
        return Ok(new { alive = true, timestamp = DateTime.UtcNow });
    }
}

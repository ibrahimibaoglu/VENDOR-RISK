using Microsoft.AspNetCore.Mvc;

namespace VendorRiskAPI.API.Controllers;

/// <summary>
/// Health check endpoint
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Basic health check
    /// </summary>
    /// <returns>Health status</returns>
    /// <response code="200">Service is healthy</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}

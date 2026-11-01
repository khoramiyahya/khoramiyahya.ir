using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Lightweight status endpoint used for connectivity checks.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class StatusController : ControllerBase
{
    private readonly IHostEnvironment _environment;

    public StatusController(IHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Returns basic API status information.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> Get()
    {
        var payload = new
        {
            status = "ok",
            service = "PersonalBrand.WebAPI",
            environment = _environment.EnvironmentName,
            utcNow = DateTimeOffset.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(payload, traceId: HttpContext.TraceIdentifier));
    }
}

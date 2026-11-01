using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Robots;
using PersonalBrand.Application.Robots.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Robots.txt endpoints for public crawler rules and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class RobotsController : ControllerBase
{
    private readonly IRobotsService _robotsService;

    public RobotsController(IRobotsService robotsService)
    {
        _robotsService = robotsService;
    }

    /// <summary>
    /// Returns the public robots.txt document.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [Produces("text/plain")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublic(CancellationToken cancellationToken)
    {
        var text = await _robotsService.GetPublicTextAsync(cancellationToken);
        return Content(text, "text/plain", System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Returns the current user's robots rules.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<RobotsRuleDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RobotsRuleDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _robotsService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<RobotsRuleDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a robots rule for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<RobotsRuleDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<RobotsRuleDto>>> CreateMine(
        [FromBody] CreateRobotsRuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _robotsService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<RobotsRuleDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's robots rules.
    /// </summary>
    [HttpPut("me/{robotsRuleId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<RobotsRuleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RobotsRuleDto>>> UpdateMine(
        Guid robotsRuleId,
        [FromBody] UpdateRobotsRuleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _robotsService.UpdateMineAsync(robotsRuleId, request, cancellationToken);
        return Ok(ApiResponse<RobotsRuleDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's robots rules.
    /// </summary>
    [HttpDelete("me/{robotsRuleId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid robotsRuleId, CancellationToken cancellationToken)
    {
        await _robotsService.DeleteMineAsync(robotsRuleId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's robots rules.
    /// </summary>
    [HttpPost("me/{robotsRuleId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<RobotsRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RobotsRuleDto>>> PublishMine(
        Guid robotsRuleId,
        CancellationToken cancellationToken)
    {
        var result = await _robotsService.PublishMineAsync(robotsRuleId, cancellationToken);
        return Ok(ApiResponse<RobotsRuleDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's robots rules.
    /// </summary>
    [HttpPost("me/{robotsRuleId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<RobotsRuleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<RobotsRuleDto>>> UnpublishMine(
        Guid robotsRuleId,
        CancellationToken cancellationToken)
    {
        var result = await _robotsService.UnpublishMineAsync(robotsRuleId, cancellationToken);
        return Ok(ApiResponse<RobotsRuleDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Awards;
using PersonalBrand.Application.Awards.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Award endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AwardsController : ControllerBase
{
    private readonly IAwardService _awardService;

    public AwardsController(IAwardService awardService)
    {
        _awardService = awardService;
    }

    /// <summary>
    /// Returns published awards.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AwardDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AwardDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _awardService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AwardDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published award by id.
    /// </summary>
    [HttpGet("public/{awardId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AwardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AwardDto>>> GetPublishedById(
        Guid awardId,
        CancellationToken cancellationToken)
    {
        var result = await _awardService.GetPublishedByIdAsync(awardId, cancellationToken);
        return Ok(ApiResponse<AwardDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's awards.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AwardDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<AwardDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _awardService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<AwardDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates an award for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AwardDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<AwardDto>>> CreateMine(
        [FromBody] CreateAwardRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _awardService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AwardDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's awards.
    /// </summary>
    [HttpPut("me/{awardId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AwardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AwardDto>>> UpdateMine(
        Guid awardId,
        [FromBody] UpdateAwardRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _awardService.UpdateMineAsync(awardId, request, cancellationToken);
        return Ok(ApiResponse<AwardDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's awards.
    /// </summary>
    [HttpDelete("me/{awardId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid awardId, CancellationToken cancellationToken)
    {
        await _awardService.DeleteMineAsync(awardId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's awards.
    /// </summary>
    [HttpPost("me/{awardId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AwardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AwardDto>>> PublishMine(
        Guid awardId,
        CancellationToken cancellationToken)
    {
        var result = await _awardService.PublishMineAsync(awardId, cancellationToken);
        return Ok(ApiResponse<AwardDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's awards.
    /// </summary>
    [HttpPost("me/{awardId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AwardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AwardDto>>> UnpublishMine(
        Guid awardId,
        CancellationToken cancellationToken)
    {
        var result = await _awardService.UnpublishMineAsync(awardId, cancellationToken);
        return Ok(ApiResponse<AwardDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

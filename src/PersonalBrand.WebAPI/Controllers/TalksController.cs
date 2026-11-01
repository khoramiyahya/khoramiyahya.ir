using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Talks;
using PersonalBrand.Application.Talks.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Talk endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class TalksController : ControllerBase
{
    private readonly ITalkService _talkService;

    public TalksController(ITalkService talkService)
    {
        _talkService = talkService;
    }

    /// <summary>
    /// Returns published talks.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TalkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TalkDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _talkService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TalkDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published talk by id.
    /// </summary>
    [HttpGet("public/{talkId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TalkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TalkDto>>> GetPublishedById(
        Guid talkId,
        CancellationToken cancellationToken)
    {
        var result = await _talkService.GetPublishedByIdAsync(talkId, cancellationToken);
        return Ok(ApiResponse<TalkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's talks.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TalkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TalkDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _talkService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TalkDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a talk for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TalkDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TalkDto>>> CreateMine(
        [FromBody] CreateTalkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _talkService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<TalkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's talks.
    /// </summary>
    [HttpPut("me/{talkId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TalkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TalkDto>>> UpdateMine(
        Guid talkId,
        [FromBody] UpdateTalkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _talkService.UpdateMineAsync(talkId, request, cancellationToken);
        return Ok(ApiResponse<TalkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's talks.
    /// </summary>
    [HttpDelete("me/{talkId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid talkId, CancellationToken cancellationToken)
    {
        await _talkService.DeleteMineAsync(talkId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's talks.
    /// </summary>
    [HttpPost("me/{talkId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TalkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TalkDto>>> PublishMine(
        Guid talkId,
        CancellationToken cancellationToken)
    {
        var result = await _talkService.PublishMineAsync(talkId, cancellationToken);
        return Ok(ApiResponse<TalkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's talks.
    /// </summary>
    [HttpPost("me/{talkId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TalkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TalkDto>>> UnpublishMine(
        Guid talkId,
        CancellationToken cancellationToken)
    {
        var result = await _talkService.UnpublishMineAsync(talkId, cancellationToken);
        return Ok(ApiResponse<TalkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

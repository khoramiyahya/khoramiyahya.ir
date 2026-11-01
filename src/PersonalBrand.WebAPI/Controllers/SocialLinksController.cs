using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.SocialLinks;
using PersonalBrand.Application.SocialLinks.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Social link endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SocialLinksController : ControllerBase
{
    private readonly ISocialLinkService _socialLinkService;

    public SocialLinksController(ISocialLinkService socialLinkService)
    {
        _socialLinkService = socialLinkService;
    }

    /// <summary>
    /// Returns published social links.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SocialLinkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SocialLinkDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _socialLinkService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SocialLinkDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published social link by id.
    /// </summary>
    [HttpGet("public/{socialLinkId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SocialLinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SocialLinkDto>>> GetPublishedById(
        Guid socialLinkId,
        CancellationToken cancellationToken)
    {
        var result = await _socialLinkService.GetPublishedByIdAsync(socialLinkId, cancellationToken);
        return Ok(ApiResponse<SocialLinkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's social links.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SocialLinkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SocialLinkDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _socialLinkService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SocialLinkDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a social link for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SocialLinkDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SocialLinkDto>>> CreateMine(
        [FromBody] CreateSocialLinkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _socialLinkService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<SocialLinkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's social links.
    /// </summary>
    [HttpPut("me/{socialLinkId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SocialLinkDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SocialLinkDto>>> UpdateMine(
        Guid socialLinkId,
        [FromBody] UpdateSocialLinkRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _socialLinkService.UpdateMineAsync(socialLinkId, request, cancellationToken);
        return Ok(ApiResponse<SocialLinkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's social links.
    /// </summary>
    [HttpDelete("me/{socialLinkId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid socialLinkId, CancellationToken cancellationToken)
    {
        await _socialLinkService.DeleteMineAsync(socialLinkId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's social links.
    /// </summary>
    [HttpPost("me/{socialLinkId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SocialLinkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SocialLinkDto>>> PublishMine(
        Guid socialLinkId,
        CancellationToken cancellationToken)
    {
        var result = await _socialLinkService.PublishMineAsync(socialLinkId, cancellationToken);
        return Ok(ApiResponse<SocialLinkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's social links.
    /// </summary>
    [HttpPost("me/{socialLinkId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SocialLinkDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SocialLinkDto>>> UnpublishMine(
        Guid socialLinkId,
        CancellationToken cancellationToken)
    {
        var result = await _socialLinkService.UnpublishMineAsync(socialLinkId, cancellationToken);
        return Ok(ApiResponse<SocialLinkDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

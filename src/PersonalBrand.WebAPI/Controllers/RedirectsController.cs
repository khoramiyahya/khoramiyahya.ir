using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Redirects;
using PersonalBrand.Application.Redirects.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// SEO redirect endpoints for public resolution and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class RedirectsController : ControllerBase
{
    private readonly IRedirectService _redirectService;

    public RedirectsController(IRedirectService redirectService)
    {
        _redirectService = redirectService;
    }

    /// <summary>
    /// Returns published SEO redirects.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SeoRedirectDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SeoRedirectDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _redirectService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SeoRedirectDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Resolves a published redirect by source path.
    /// </summary>
    [HttpGet("public/resolve")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SeoRedirectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SeoRedirectDto>>> ResolvePublished(
        [FromQuery] string fromPath,
        CancellationToken cancellationToken)
    {
        var result = await _redirectService.ResolvePublishedByPathAsync(fromPath, cancellationToken);
        return Ok(ApiResponse<SeoRedirectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's SEO redirects.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SeoRedirectDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SeoRedirectDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _redirectService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SeoRedirectDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates an SEO redirect for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SeoRedirectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SeoRedirectDto>>> CreateMine(
        [FromBody] CreateSeoRedirectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _redirectService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<SeoRedirectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's SEO redirects.
    /// </summary>
    [HttpPut("me/{redirectId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SeoRedirectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SeoRedirectDto>>> UpdateMine(
        Guid redirectId,
        [FromBody] UpdateSeoRedirectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _redirectService.UpdateMineAsync(redirectId, request, cancellationToken);
        return Ok(ApiResponse<SeoRedirectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's SEO redirects.
    /// </summary>
    [HttpDelete("me/{redirectId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid redirectId, CancellationToken cancellationToken)
    {
        await _redirectService.DeleteMineAsync(redirectId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's SEO redirects.
    /// </summary>
    [HttpPost("me/{redirectId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SeoRedirectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SeoRedirectDto>>> PublishMine(
        Guid redirectId,
        CancellationToken cancellationToken)
    {
        var result = await _redirectService.PublishMineAsync(redirectId, cancellationToken);
        return Ok(ApiResponse<SeoRedirectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's SEO redirects.
    /// </summary>
    [HttpPost("me/{redirectId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SeoRedirectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SeoRedirectDto>>> UnpublishMine(
        Guid redirectId,
        CancellationToken cancellationToken)
    {
        var result = await _redirectService.UnpublishMineAsync(redirectId, cancellationToken);
        return Ok(ApiResponse<SeoRedirectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

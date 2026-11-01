using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Pages;
using PersonalBrand.Application.Pages.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Content page endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PagesController : ControllerBase
{
    private readonly IPageService _pageService;

    public PagesController(IPageService pageService)
    {
        _pageService = pageService;
    }

    /// <summary>
    /// Returns published content pages.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ContentPageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContentPageDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _pageService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ContentPageDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published content page by slug.
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ContentPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ContentPageDto>>> GetPublishedBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var result = await _pageService.GetPublishedBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse<ContentPageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's content pages.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ContentPageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContentPageDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _pageService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ContentPageDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a content page for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ContentPageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ContentPageDto>>> CreateMine(
        [FromBody] CreateContentPageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _pageService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ContentPageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's content pages.
    /// </summary>
    [HttpPut("me/{pageId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ContentPageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ContentPageDto>>> UpdateMine(
        Guid pageId,
        [FromBody] UpdateContentPageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _pageService.UpdateMineAsync(pageId, request, cancellationToken);
        return Ok(ApiResponse<ContentPageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's content pages.
    /// </summary>
    [HttpDelete("me/{pageId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid pageId, CancellationToken cancellationToken)
    {
        await _pageService.DeleteMineAsync(pageId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's content pages.
    /// </summary>
    [HttpPost("me/{pageId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ContentPageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ContentPageDto>>> PublishMine(
        Guid pageId,
        CancellationToken cancellationToken)
    {
        var result = await _pageService.PublishMineAsync(pageId, cancellationToken);
        return Ok(ApiResponse<ContentPageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's content pages.
    /// </summary>
    [HttpPost("me/{pageId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ContentPageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ContentPageDto>>> UnpublishMine(
        Guid pageId,
        CancellationToken cancellationToken)
    {
        var result = await _pageService.UnpublishMineAsync(pageId, cancellationToken);
        return Ok(ApiResponse<ContentPageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

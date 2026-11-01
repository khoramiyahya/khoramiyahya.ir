using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Faq;
using PersonalBrand.Application.Faq.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// FAQ endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class FaqController : ControllerBase
{
    private readonly IFaqService _faqService;

    public FaqController(IFaqService faqService)
    {
        _faqService = faqService;
    }

    /// <summary>
    /// Returns published FAQ items.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FaqItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FaqItemDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _faqService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FaqItemDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published FAQ item by id.
    /// </summary>
    [HttpGet("public/{faqId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FaqItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FaqItemDto>>> GetPublishedById(
        Guid faqId,
        CancellationToken cancellationToken)
    {
        var result = await _faqService.GetPublishedByIdAsync(faqId, cancellationToken);
        return Ok(ApiResponse<FaqItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's FAQ items.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FaqItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<FaqItemDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _faqService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<FaqItemDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates an FAQ item for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FaqItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<FaqItemDto>>> CreateMine(
        [FromBody] CreateFaqItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _faqService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<FaqItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's FAQ items.
    /// </summary>
    [HttpPut("me/{faqId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FaqItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FaqItemDto>>> UpdateMine(
        Guid faqId,
        [FromBody] UpdateFaqItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _faqService.UpdateMineAsync(faqId, request, cancellationToken);
        return Ok(ApiResponse<FaqItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's FAQ items.
    /// </summary>
    [HttpDelete("me/{faqId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid faqId, CancellationToken cancellationToken)
    {
        await _faqService.DeleteMineAsync(faqId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's FAQ items.
    /// </summary>
    [HttpPost("me/{faqId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FaqItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FaqItemDto>>> PublishMine(
        Guid faqId,
        CancellationToken cancellationToken)
    {
        var result = await _faqService.PublishMineAsync(faqId, cancellationToken);
        return Ok(ApiResponse<FaqItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's FAQ items.
    /// </summary>
    [HttpPost("me/{faqId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<FaqItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FaqItemDto>>> UnpublishMine(
        Guid faqId,
        CancellationToken cancellationToken)
    {
        var result = await _faqService.UnpublishMineAsync(faqId, cancellationToken);
        return Ok(ApiResponse<FaqItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Publications;
using PersonalBrand.Application.Publications.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Publication endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PublicationsController : ControllerBase
{
    private readonly IPublicationService _publicationService;

    public PublicationsController(IPublicationService publicationService)
    {
        _publicationService = publicationService;
    }

    /// <summary>
    /// Returns published publications.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PublicationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PublicationDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _publicationService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PublicationDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published publication by id.
    /// </summary>
    [HttpGet("public/{publicationId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PublicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PublicationDto>>> GetPublishedById(
        Guid publicationId,
        CancellationToken cancellationToken)
    {
        var result = await _publicationService.GetPublishedByIdAsync(publicationId, cancellationToken);
        return Ok(ApiResponse<PublicationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's publications.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PublicationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PublicationDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _publicationService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PublicationDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a publication for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PublicationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PublicationDto>>> CreateMine(
        [FromBody] CreatePublicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _publicationService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<PublicationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's publications.
    /// </summary>
    [HttpPut("me/{publicationId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PublicationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PublicationDto>>> UpdateMine(
        Guid publicationId,
        [FromBody] UpdatePublicationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _publicationService.UpdateMineAsync(publicationId, request, cancellationToken);
        return Ok(ApiResponse<PublicationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's publications.
    /// </summary>
    [HttpDelete("me/{publicationId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid publicationId, CancellationToken cancellationToken)
    {
        await _publicationService.DeleteMineAsync(publicationId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's publications.
    /// </summary>
    [HttpPost("me/{publicationId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PublicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PublicationDto>>> PublishMine(
        Guid publicationId,
        CancellationToken cancellationToken)
    {
        var result = await _publicationService.PublishMineAsync(publicationId, cancellationToken);
        return Ok(ApiResponse<PublicationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's publications.
    /// </summary>
    [HttpPost("me/{publicationId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PublicationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PublicationDto>>> UnpublishMine(
        Guid publicationId,
        CancellationToken cancellationToken)
    {
        var result = await _publicationService.UnpublishMineAsync(publicationId, cancellationToken);
        return Ok(ApiResponse<PublicationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

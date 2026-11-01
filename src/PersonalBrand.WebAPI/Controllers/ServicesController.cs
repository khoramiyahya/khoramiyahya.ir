using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Services;
using PersonalBrand.Application.Services.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Service catalog endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ServicesController : ControllerBase
{
    private readonly IServiceOfferService _serviceOfferService;

    public ServicesController(IServiceOfferService serviceOfferService)
    {
        _serviceOfferService = serviceOfferService;
    }

    /// <summary>
    /// Returns published service offerings.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ServiceOfferDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ServiceOfferDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _serviceOfferService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ServiceOfferDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published service offering by slug.
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ServiceOfferDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServiceOfferDto>>> GetPublishedBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOfferService.GetPublishedBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse<ServiceOfferDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's service offerings.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ServiceOfferDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ServiceOfferDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _serviceOfferService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ServiceOfferDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a service offering for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ServiceOfferDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ServiceOfferDto>>> CreateMine(
        [FromBody] CreateServiceOfferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOfferService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ServiceOfferDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's service offerings.
    /// </summary>
    [HttpPut("me/{serviceId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ServiceOfferDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServiceOfferDto>>> UpdateMine(
        Guid serviceId,
        [FromBody] UpdateServiceOfferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOfferService.UpdateMineAsync(serviceId, request, cancellationToken);
        return Ok(ApiResponse<ServiceOfferDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's service offerings.
    /// </summary>
    [HttpDelete("me/{serviceId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid serviceId, CancellationToken cancellationToken)
    {
        await _serviceOfferService.DeleteMineAsync(serviceId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's service offerings.
    /// </summary>
    [HttpPost("me/{serviceId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ServiceOfferDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceOfferDto>>> PublishMine(
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOfferService.PublishMineAsync(serviceId, cancellationToken);
        return Ok(ApiResponse<ServiceOfferDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's service offerings.
    /// </summary>
    [HttpPost("me/{serviceId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ServiceOfferDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ServiceOfferDto>>> UnpublishMine(
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var result = await _serviceOfferService.UnpublishMineAsync(serviceId, cancellationToken);
        return Ok(ApiResponse<ServiceOfferDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

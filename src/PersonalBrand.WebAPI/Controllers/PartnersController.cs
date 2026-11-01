using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Partners;
using PersonalBrand.Application.Partners.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Partner endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class PartnersController : ControllerBase
{
    private readonly IPartnerService _partnerService;

    public PartnersController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    /// <summary>
    /// Returns published partners.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PartnerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PartnerDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _partnerService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PartnerDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published partner by id.
    /// </summary>
    [HttpGet("public/{partnerId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PartnerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PartnerDto>>> GetPublishedById(
        Guid partnerId,
        CancellationToken cancellationToken)
    {
        var result = await _partnerService.GetPublishedByIdAsync(partnerId, cancellationToken);
        return Ok(ApiResponse<PartnerDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's partners.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PartnerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PartnerDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _partnerService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PartnerDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a partner for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PartnerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PartnerDto>>> CreateMine(
        [FromBody] CreatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _partnerService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<PartnerDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's partners.
    /// </summary>
    [HttpPut("me/{partnerId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PartnerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PartnerDto>>> UpdateMine(
        Guid partnerId,
        [FromBody] UpdatePartnerRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _partnerService.UpdateMineAsync(partnerId, request, cancellationToken);
        return Ok(ApiResponse<PartnerDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's partners.
    /// </summary>
    [HttpDelete("me/{partnerId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid partnerId, CancellationToken cancellationToken)
    {
        await _partnerService.DeleteMineAsync(partnerId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's partners.
    /// </summary>
    [HttpPost("me/{partnerId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PartnerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PartnerDto>>> PublishMine(
        Guid partnerId,
        CancellationToken cancellationToken)
    {
        var result = await _partnerService.PublishMineAsync(partnerId, cancellationToken);
        return Ok(ApiResponse<PartnerDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's partners.
    /// </summary>
    [HttpPost("me/{partnerId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PartnerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PartnerDto>>> UnpublishMine(
        Guid partnerId,
        CancellationToken cancellationToken)
    {
        var result = await _partnerService.UnpublishMineAsync(partnerId, cancellationToken);
        return Ok(ApiResponse<PartnerDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

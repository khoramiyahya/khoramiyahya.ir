using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Certifications;
using PersonalBrand.Application.Certifications.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Certification endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class CertificationsController : ControllerBase
{
    private readonly ICertificationService _certificationService;

    public CertificationsController(ICertificationService certificationService)
    {
        _certificationService = certificationService;
    }

    /// <summary>
    /// Returns published certifications.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CertificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CertificationDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _certificationService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CertificationDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published certification by id.
    /// </summary>
    [HttpGet("public/{certificationId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CertificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CertificationDto>>> GetPublishedById(
        Guid certificationId,
        CancellationToken cancellationToken)
    {
        var result = await _certificationService.GetPublishedByIdAsync(certificationId, cancellationToken);
        return Ok(ApiResponse<CertificationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's certifications.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CertificationDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<CertificationDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _certificationService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<CertificationDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a certification for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CertificationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<CertificationDto>>> CreateMine(
        [FromBody] CreateCertificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _certificationService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<CertificationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's certifications.
    /// </summary>
    [HttpPut("me/{certificationId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CertificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CertificationDto>>> UpdateMine(
        Guid certificationId,
        [FromBody] UpdateCertificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _certificationService.UpdateMineAsync(certificationId, request, cancellationToken);
        return Ok(ApiResponse<CertificationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's certifications.
    /// </summary>
    [HttpDelete("me/{certificationId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid certificationId, CancellationToken cancellationToken)
    {
        await _certificationService.DeleteMineAsync(certificationId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's certifications.
    /// </summary>
    [HttpPost("me/{certificationId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CertificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CertificationDto>>> PublishMine(
        Guid certificationId,
        CancellationToken cancellationToken)
    {
        var result = await _certificationService.PublishMineAsync(certificationId, cancellationToken);
        return Ok(ApiResponse<CertificationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's certifications.
    /// </summary>
    [HttpPost("me/{certificationId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CertificationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<CertificationDto>>> UnpublishMine(
        Guid certificationId,
        CancellationToken cancellationToken)
    {
        var result = await _certificationService.UnpublishMineAsync(certificationId, cancellationToken);
        return Ok(ApiResponse<CertificationDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

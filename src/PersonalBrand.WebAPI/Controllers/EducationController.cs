using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Education;
using PersonalBrand.Application.Education.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Education endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class EducationController : ControllerBase
{
    private readonly IEducationService _educationService;

    public EducationController(IEducationService educationService)
    {
        _educationService = educationService;
    }

    /// <summary>
    /// Returns published education entries.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EducationEntryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EducationEntryDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _educationService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EducationEntryDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published education entry by id.
    /// </summary>
    [HttpGet("public/{educationId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<EducationEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EducationEntryDto>>> GetPublishedById(
        Guid educationId,
        CancellationToken cancellationToken)
    {
        var result = await _educationService.GetPublishedByIdAsync(educationId, cancellationToken);
        return Ok(ApiResponse<EducationEntryDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's education entries.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EducationEntryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<EducationEntryDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _educationService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<EducationEntryDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates an education entry for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EducationEntryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<EducationEntryDto>>> CreateMine(
        [FromBody] CreateEducationEntryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _educationService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<EducationEntryDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's education entries.
    /// </summary>
    [HttpPut("me/{educationId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EducationEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<EducationEntryDto>>> UpdateMine(
        Guid educationId,
        [FromBody] UpdateEducationEntryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _educationService.UpdateMineAsync(educationId, request, cancellationToken);
        return Ok(ApiResponse<EducationEntryDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's education entries.
    /// </summary>
    [HttpDelete("me/{educationId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid educationId, CancellationToken cancellationToken)
    {
        await _educationService.DeleteMineAsync(educationId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's education entries.
    /// </summary>
    [HttpPost("me/{educationId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EducationEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EducationEntryDto>>> PublishMine(
        Guid educationId,
        CancellationToken cancellationToken)
    {
        var result = await _educationService.PublishMineAsync(educationId, cancellationToken);
        return Ok(ApiResponse<EducationEntryDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's education entries.
    /// </summary>
    [HttpPost("me/{educationId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<EducationEntryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EducationEntryDto>>> UnpublishMine(
        Guid educationId,
        CancellationToken cancellationToken)
    {
        var result = await _educationService.UnpublishMineAsync(educationId, cancellationToken);
        return Ok(ApiResponse<EducationEntryDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

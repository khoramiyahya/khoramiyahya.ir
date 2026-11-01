using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Experience;
using PersonalBrand.Application.Experience.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Work experience endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ExperienceController : ControllerBase
{
    private readonly IExperienceService _experienceService;

    public ExperienceController(IExperienceService experienceService)
    {
        _experienceService = experienceService;
    }

    /// <summary>
    /// Returns published work experience entries.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkExperienceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkExperienceDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _experienceService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WorkExperienceDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published work experience entry by id.
    /// </summary>
    [HttpGet("public/{experienceId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<WorkExperienceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WorkExperienceDto>>> GetPublishedById(
        Guid experienceId,
        CancellationToken cancellationToken)
    {
        var result = await _experienceService.GetPublishedByIdAsync(experienceId, cancellationToken);
        return Ok(ApiResponse<WorkExperienceDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's work experience entries.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<WorkExperienceDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WorkExperienceDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _experienceService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WorkExperienceDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a work experience entry for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<WorkExperienceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<WorkExperienceDto>>> CreateMine(
        [FromBody] CreateWorkExperienceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _experienceService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<WorkExperienceDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's work experience entries.
    /// </summary>
    [HttpPut("me/{experienceId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<WorkExperienceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<WorkExperienceDto>>> UpdateMine(
        Guid experienceId,
        [FromBody] UpdateWorkExperienceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _experienceService.UpdateMineAsync(experienceId, request, cancellationToken);
        return Ok(ApiResponse<WorkExperienceDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's work experience entries.
    /// </summary>
    [HttpDelete("me/{experienceId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid experienceId, CancellationToken cancellationToken)
    {
        await _experienceService.DeleteMineAsync(experienceId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's work experience entries.
    /// </summary>
    [HttpPost("me/{experienceId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<WorkExperienceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WorkExperienceDto>>> PublishMine(
        Guid experienceId,
        CancellationToken cancellationToken)
    {
        var result = await _experienceService.PublishMineAsync(experienceId, cancellationToken);
        return Ok(ApiResponse<WorkExperienceDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's work experience entries.
    /// </summary>
    [HttpPost("me/{experienceId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<WorkExperienceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WorkExperienceDto>>> UnpublishMine(
        Guid experienceId,
        CancellationToken cancellationToken)
    {
        var result = await _experienceService.UnpublishMineAsync(experienceId, cancellationToken);
        return Ok(ApiResponse<WorkExperienceDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

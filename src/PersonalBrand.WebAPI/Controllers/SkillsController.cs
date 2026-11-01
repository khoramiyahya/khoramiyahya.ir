using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Skills;
using PersonalBrand.Application.Skills.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Skill endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;

    public SkillsController(ISkillService skillService)
    {
        _skillService = skillService;
    }

    /// <summary>
    /// Returns published skills.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SkillDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SkillDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _skillService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SkillDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published skill by id.
    /// </summary>
    [HttpGet("public/{skillId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SkillDto>>> GetPublishedById(
        Guid skillId,
        CancellationToken cancellationToken)
    {
        var result = await _skillService.GetPublishedByIdAsync(skillId, cancellationToken);
        return Ok(ApiResponse<SkillDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's skills.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SkillDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SkillDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _skillService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SkillDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a skill for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SkillDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SkillDto>>> CreateMine(
        [FromBody] CreateSkillRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _skillService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<SkillDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's skills.
    /// </summary>
    [HttpPut("me/{skillId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SkillDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SkillDto>>> UpdateMine(
        Guid skillId,
        [FromBody] UpdateSkillRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _skillService.UpdateMineAsync(skillId, request, cancellationToken);
        return Ok(ApiResponse<SkillDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's skills.
    /// </summary>
    [HttpDelete("me/{skillId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid skillId, CancellationToken cancellationToken)
    {
        await _skillService.DeleteMineAsync(skillId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's skills.
    /// </summary>
    [HttpPost("me/{skillId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SkillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SkillDto>>> PublishMine(
        Guid skillId,
        CancellationToken cancellationToken)
    {
        var result = await _skillService.PublishMineAsync(skillId, cancellationToken);
        return Ok(ApiResponse<SkillDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's skills.
    /// </summary>
    [HttpPost("me/{skillId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SkillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SkillDto>>> UnpublishMine(
        Guid skillId,
        CancellationToken cancellationToken)
    {
        var result = await _skillService.UnpublishMineAsync(skillId, cancellationToken);
        return Ok(ApiResponse<SkillDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

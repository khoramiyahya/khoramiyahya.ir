using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Projects;
using PersonalBrand.Application.Projects.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Portfolio project endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Returns published portfolio projects.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProjectDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _projectService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProjectDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published project by slug.
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetPublishedBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.GetPublishedBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's projects.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ProjectDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ProjectDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _projectService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ProjectDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a portfolio project for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateMine(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ProjectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's projects.
    /// </summary>
    [HttpPut("me/{projectId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateMine(
        Guid projectId,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateMineAsync(projectId, request, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's projects.
    /// </summary>
    [HttpDelete("me/{projectId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid projectId, CancellationToken cancellationToken)
    {
        await _projectService.DeleteMineAsync(projectId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's projects.
    /// </summary>
    [HttpPost("me/{projectId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> PublishMine(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.PublishMineAsync(projectId, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's projects.
    /// </summary>
    [HttpPost("me/{projectId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UnpublishMine(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.UnpublishMineAsync(projectId, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Uploads or replaces the cover image for one of the current user's projects.
    /// </summary>
    [HttpPost("me/{projectId:guid}/cover")]
    [Authorize]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UploadCover(
        Guid projectId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Cover file is required.",
                traceId: HttpContext.TraceIdentifier));
        }

        await using var stream = file.OpenReadStream();
        var result = await _projectService.UploadCoverAsync(
            projectId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        return Ok(ApiResponse<ProjectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Removes the cover image for one of the current user's projects.
    /// </summary>
    [HttpDelete("me/{projectId:guid}/cover")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> RemoveCover(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.RemoveCoverAsync(projectId, cancellationToken);
        return Ok(ApiResponse<ProjectDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

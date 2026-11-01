using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Profiles;
using PersonalBrand.Application.Profiles.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Brand profile endpoints for public viewing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfilesController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    /// <summary>
    /// Returns the current user's profile, creating a draft if needed.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _profileService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<ProfileDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates or updates the current user's brand profile.
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpsertMine(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _profileService.UpsertMineAsync(request, cancellationToken);
        return Ok(ApiResponse<ProfileDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Publishes the current user's profile.
    /// </summary>
    [HttpPost("me/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> PublishMine(CancellationToken cancellationToken)
    {
        var result = await _profileService.PublishMineAsync(cancellationToken);
        return Ok(ApiResponse<ProfileDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes the current user's profile.
    /// </summary>
    [HttpPost("me/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UnpublishMine(CancellationToken cancellationToken)
    {
        var result = await _profileService.UnpublishMineAsync(cancellationToken);
        return Ok(ApiResponse<ProfileDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published profile by public slug.
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var result = await _profileService.GetPublishedBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse<ProfileDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}


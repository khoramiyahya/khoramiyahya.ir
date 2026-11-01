using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Media;
using PersonalBrand.Application.Media.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Media library endpoints for owner-managed assets.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public sealed class MediaController : ControllerBase
{
    private readonly IMediaService _mediaService;

    public MediaController(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    /// <summary>
    /// Returns the current user's media assets.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MediaAssetDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MediaAssetDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _mediaService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MediaAssetDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns one media asset owned by the current user.
    /// </summary>
    [HttpGet("me/{mediaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MediaAssetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MediaAssetDto>>> GetMineById(
        Guid mediaId,
        CancellationToken cancellationToken)
    {
        var result = await _mediaService.GetMineByIdAsync(mediaId, cancellationToken);
        return Ok(ApiResponse<MediaAssetDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Uploads a media asset for the current user.
    /// </summary>
    [HttpPost("me")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<MediaAssetDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<MediaAssetDto>>> UploadMine(
        IFormFile file,
        [FromForm] string? title,
        [FromForm] string? altText,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Media file is required.",
                traceId: HttpContext.TraceIdentifier));
        }

        await using var stream = file.OpenReadStream();
        var result = await _mediaService.UploadMineAsync(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            title,
            altText,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<MediaAssetDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates metadata for one of the current user's media assets.
    /// </summary>
    [HttpPut("me/{mediaId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<MediaAssetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<MediaAssetDto>>> UpdateMine(
        Guid mediaId,
        [FromBody] UpdateMediaAssetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediaService.UpdateMineAsync(mediaId, request, cancellationToken);
        return Ok(ApiResponse<MediaAssetDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's media assets.
    /// </summary>
    [HttpDelete("me/{mediaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid mediaId, CancellationToken cancellationToken)
    {
        await _mediaService.DeleteMineAsync(mediaId, cancellationToken);
        return NoContent();
    }
}

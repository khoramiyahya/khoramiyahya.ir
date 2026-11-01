using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Languages;
using PersonalBrand.Application.Languages.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Spoken language endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class LanguagesController : ControllerBase
{
    private readonly ILanguageService _languageService;

    public LanguagesController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    /// <summary>
    /// Returns published spoken languages.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SpokenLanguageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SpokenLanguageDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _languageService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SpokenLanguageDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published spoken language by id.
    /// </summary>
    [HttpGet("public/{languageId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SpokenLanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SpokenLanguageDto>>> GetPublishedById(
        Guid languageId,
        CancellationToken cancellationToken)
    {
        var result = await _languageService.GetPublishedByIdAsync(languageId, cancellationToken);
        return Ok(ApiResponse<SpokenLanguageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's spoken languages.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SpokenLanguageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SpokenLanguageDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _languageService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SpokenLanguageDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a spoken language for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SpokenLanguageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SpokenLanguageDto>>> CreateMine(
        [FromBody] CreateSpokenLanguageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _languageService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<SpokenLanguageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's spoken languages.
    /// </summary>
    [HttpPut("me/{languageId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SpokenLanguageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SpokenLanguageDto>>> UpdateMine(
        Guid languageId,
        [FromBody] UpdateSpokenLanguageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _languageService.UpdateMineAsync(languageId, request, cancellationToken);
        return Ok(ApiResponse<SpokenLanguageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's spoken languages.
    /// </summary>
    [HttpDelete("me/{languageId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid languageId, CancellationToken cancellationToken)
    {
        await _languageService.DeleteMineAsync(languageId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's spoken languages.
    /// </summary>
    [HttpPost("me/{languageId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SpokenLanguageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SpokenLanguageDto>>> PublishMine(
        Guid languageId,
        CancellationToken cancellationToken)
    {
        var result = await _languageService.PublishMineAsync(languageId, cancellationToken);
        return Ok(ApiResponse<SpokenLanguageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's spoken languages.
    /// </summary>
    [HttpPost("me/{languageId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SpokenLanguageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SpokenLanguageDto>>> UnpublishMine(
        Guid languageId,
        CancellationToken cancellationToken)
    {
        var result = await _languageService.UnpublishMineAsync(languageId, cancellationToken);
        return Ok(ApiResponse<SpokenLanguageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Settings;
using PersonalBrand.Application.Settings.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Site settings endpoints for public SEO data and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISiteSettingsService _siteSettingsService;

    public SettingsController(ISiteSettingsService siteSettingsService)
    {
        _siteSettingsService = siteSettingsService;
    }

    /// <summary>
    /// Returns public site settings used by the frontend.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SiteSettingsDto>>> GetPublic(
        CancellationToken cancellationToken)
    {
        var result = await _siteSettingsService.GetPublicAsync(cancellationToken);
        return Ok(ApiResponse<SiteSettingsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns site settings for the authenticated owner.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SiteSettingsDto>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _siteSettingsService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<SiteSettingsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates site settings for the authenticated owner.
    /// </summary>
    [HttpPut("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SiteSettingsDto>>> UpdateMine(
        [FromBody] UpdateSiteSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _siteSettingsService.UpdateMineAsync(request, cancellationToken);
        return Ok(ApiResponse<SiteSettingsDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

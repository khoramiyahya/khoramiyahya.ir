using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Sitemap;
using PersonalBrand.Application.Sitemap.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Public sitemap endpoints for SEO and frontend routing maps.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SitemapController : ControllerBase
{
    private readonly ISitemapService _sitemapService;

    public SitemapController(ISitemapService sitemapService)
    {
        _sitemapService = sitemapService;
    }

    /// <summary>
    /// Returns the public sitemap as JSON.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SitemapDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SitemapDto>>> GetJson(CancellationToken cancellationToken)
    {
        var result = await _sitemapService.GetPublicAsync(cancellationToken);
        return Ok(ApiResponse<SitemapDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the public sitemap as XML.
    /// </summary>
    [HttpGet("xml")]
    [AllowAnonymous]
    [Produces("application/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetXml(CancellationToken cancellationToken)
    {
        var xml = await _sitemapService.GetPublicXmlAsync(cancellationToken);
        return Content(xml, "application/xml", System.Text.Encoding.UTF8);
    }
}

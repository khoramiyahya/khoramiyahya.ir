using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Search;
using PersonalBrand.Application.Search.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Public site-wide search endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Searches published content across projects, blog, services, pages, FAQ, skills, certifications, experience, education, awards, talks, publications, and languages.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SearchResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<SearchResponseDto>>> Search(
        [FromQuery] string q,
        [FromQuery] string? contentType,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _searchService.SearchPublishedAsync(
            new SearchRequest
            {
                Query = q,
                ContentType = contentType,
                Limit = limit
            },
            cancellationToken);

        return Ok(ApiResponse<SearchResponseDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

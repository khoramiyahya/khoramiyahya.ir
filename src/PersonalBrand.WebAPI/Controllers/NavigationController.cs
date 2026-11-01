using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Navigation;
using PersonalBrand.Application.Navigation.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Navigation menu endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class NavigationController : ControllerBase
{
    private readonly INavigationService _navigationService;

    public NavigationController(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }

    /// <summary>
    /// Returns published navigation items, optionally filtered by location.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NavigationItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NavigationItemDto>>>> GetPublished(
        [FromQuery] string? location,
        CancellationToken cancellationToken)
    {
        var result = await _navigationService.GetPublishedAsync(location, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NavigationItemDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published navigation item by id.
    /// </summary>
    [HttpGet("public/{navigationItemId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<NavigationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NavigationItemDto>>> GetPublishedById(
        Guid navigationItemId,
        CancellationToken cancellationToken)
    {
        var result = await _navigationService.GetPublishedByIdAsync(navigationItemId, cancellationToken);
        return Ok(ApiResponse<NavigationItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's navigation items.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NavigationItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NavigationItemDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _navigationService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NavigationItemDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a navigation item for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<NavigationItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<NavigationItemDto>>> CreateMine(
        [FromBody] CreateNavigationItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _navigationService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<NavigationItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's navigation items.
    /// </summary>
    [HttpPut("me/{navigationItemId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<NavigationItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NavigationItemDto>>> UpdateMine(
        Guid navigationItemId,
        [FromBody] UpdateNavigationItemRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _navigationService.UpdateMineAsync(navigationItemId, request, cancellationToken);
        return Ok(ApiResponse<NavigationItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's navigation items.
    /// </summary>
    [HttpDelete("me/{navigationItemId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid navigationItemId, CancellationToken cancellationToken)
    {
        await _navigationService.DeleteMineAsync(navigationItemId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's navigation items.
    /// </summary>
    [HttpPost("me/{navigationItemId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<NavigationItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<NavigationItemDto>>> PublishMine(
        Guid navigationItemId,
        CancellationToken cancellationToken)
    {
        var result = await _navigationService.PublishMineAsync(navigationItemId, cancellationToken);
        return Ok(ApiResponse<NavigationItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's navigation items.
    /// </summary>
    [HttpPost("me/{navigationItemId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<NavigationItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<NavigationItemDto>>> UnpublishMine(
        Guid navigationItemId,
        CancellationToken cancellationToken)
    {
        var result = await _navigationService.UnpublishMineAsync(navigationItemId, cancellationToken);
        return Ok(ApiResponse<NavigationItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

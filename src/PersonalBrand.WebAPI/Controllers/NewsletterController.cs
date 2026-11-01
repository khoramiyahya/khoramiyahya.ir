using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using PersonalBrand.Application.Newsletter;
using PersonalBrand.Application.Newsletter.Contracts;
using PersonalBrand.WebAPI.Contracts;
using PersonalBrand.WebAPI.Extensions;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Public newsletter subscription and owner subscriber management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class NewsletterController : ControllerBase
{
    private readonly INewsletterService _newsletterService;

    public NewsletterController(INewsletterService newsletterService)
    {
        _newsletterService = newsletterService;
    }

    /// <summary>
    /// Subscribes an email to the newsletter.
    /// </summary>
    [HttpPost("subscribe")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.NewsletterPolicyName)]
    [ProducesResponseType(typeof(ApiResponse<NewsletterSubscriberDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<NewsletterSubscriberDto>>> Subscribe(
        [FromBody] SubscribeNewsletterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _newsletterService.SubscribeAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<NewsletterSubscriberDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unsubscribes an email from the newsletter.
    /// </summary>
    [HttpPost("unsubscribe")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.NewsletterPolicyName)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Unsubscribe(
        [FromBody] UnsubscribeNewsletterRequest request,
        CancellationToken cancellationToken)
    {
        await _newsletterService.UnsubscribeAsync(request, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Returns newsletter subscribers for the authenticated owner.
    /// </summary>
    [HttpGet("subscribers")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NewsletterSubscriberDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<NewsletterSubscriberDto>>>> GetSubscribers(
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _newsletterService.GetSubscribersAsync(activeOnly, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<NewsletterSubscriberDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes a newsletter subscriber.
    /// </summary>
    [HttpDelete("subscribers/{subscriberId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubscriber(Guid subscriberId, CancellationToken cancellationToken)
    {
        await _newsletterService.DeleteSubscriberAsync(subscriberId, cancellationToken);
        return NoContent();
    }
}

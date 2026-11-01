using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using PersonalBrand.Application.Contact;
using PersonalBrand.Application.Contact.Contracts;
using PersonalBrand.WebAPI.Contracts;
using PersonalBrand.WebAPI.Extensions;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Public contact form and authenticated inbox endpoints.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ContactController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactController(IContactService contactService)
    {
        _contactService = contactService;
    }

    /// <summary>
    /// Submits a public contact form message.
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.ContactPolicyName)]
    [ProducesResponseType(typeof(ApiResponse<ContactMessageDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ContactMessageDto>>> Submit(
        [FromBody] SubmitContactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.SubmitAsync(
            request,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers.UserAgent.ToString(),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<ContactMessageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns contact inbox messages for the authenticated owner.
    /// </summary>
    [HttpGet("inbox")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ContactMessageDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ContactMessageDto>>>> GetInbox(
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _contactService.GetInboxAsync(unreadOnly, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ContactMessageDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns one contact message by id.
    /// </summary>
    [HttpGet("inbox/{messageId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ContactMessageDto>>> GetById(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.GetByIdAsync(messageId, cancellationToken);
        return Ok(ApiResponse<ContactMessageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Marks a contact message as read.
    /// </summary>
    [HttpPost("inbox/{messageId:guid}/read")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ContactMessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ContactMessageDto>>> MarkRead(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.MarkReadAsync(messageId, cancellationToken);
        return Ok(ApiResponse<ContactMessageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Marks a contact message as unread.
    /// </summary>
    [HttpPost("inbox/{messageId:guid}/unread")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ContactMessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ContactMessageDto>>> MarkUnread(
        Guid messageId,
        CancellationToken cancellationToken)
    {
        var result = await _contactService.MarkUnreadAsync(messageId, cancellationToken);
        return Ok(ApiResponse<ContactMessageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes a contact message.
    /// </summary>
    [HttpDelete("inbox/{messageId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid messageId, CancellationToken cancellationToken)
    {
        await _contactService.DeleteAsync(messageId, cancellationToken);
        return NoContent();
    }
}

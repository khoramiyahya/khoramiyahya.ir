using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Testimonials;
using PersonalBrand.Application.Testimonials.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Testimonial endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class TestimonialsController : ControllerBase
{
    private readonly ITestimonialService _testimonialService;

    public TestimonialsController(ITestimonialService testimonialService)
    {
        _testimonialService = testimonialService;
    }

    /// <summary>
    /// Returns published testimonials.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TestimonialDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TestimonialDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _testimonialService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TestimonialDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published testimonial by id.
    /// </summary>
    [HttpGet("public/{testimonialId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<TestimonialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TestimonialDto>>> GetPublishedById(
        Guid testimonialId,
        CancellationToken cancellationToken)
    {
        var result = await _testimonialService.GetPublishedByIdAsync(testimonialId, cancellationToken);
        return Ok(ApiResponse<TestimonialDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's testimonials.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TestimonialDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TestimonialDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _testimonialService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TestimonialDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a testimonial for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TestimonialDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<TestimonialDto>>> CreateMine(
        [FromBody] CreateTestimonialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _testimonialService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<TestimonialDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's testimonials.
    /// </summary>
    [HttpPut("me/{testimonialId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TestimonialDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TestimonialDto>>> UpdateMine(
        Guid testimonialId,
        [FromBody] UpdateTestimonialRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _testimonialService.UpdateMineAsync(testimonialId, request, cancellationToken);
        return Ok(ApiResponse<TestimonialDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's testimonials.
    /// </summary>
    [HttpDelete("me/{testimonialId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid testimonialId, CancellationToken cancellationToken)
    {
        await _testimonialService.DeleteMineAsync(testimonialId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's testimonials.
    /// </summary>
    [HttpPost("me/{testimonialId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TestimonialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TestimonialDto>>> PublishMine(
        Guid testimonialId,
        CancellationToken cancellationToken)
    {
        var result = await _testimonialService.PublishMineAsync(testimonialId, cancellationToken);
        return Ok(ApiResponse<TestimonialDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's testimonials.
    /// </summary>
    [HttpPost("me/{testimonialId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<TestimonialDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TestimonialDto>>> UnpublishMine(
        Guid testimonialId,
        CancellationToken cancellationToken)
    {
        var result = await _testimonialService.UnpublishMineAsync(testimonialId, cancellationToken);
        return Ok(ApiResponse<TestimonialDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

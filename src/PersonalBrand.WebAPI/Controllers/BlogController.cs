using Asp.Versioning;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using PersonalBrand.Application.Blog;
using PersonalBrand.Application.Blog.Contracts;
using PersonalBrand.WebAPI.Contracts;

namespace PersonalBrand.WebAPI.Controllers;

/// <summary>
/// Blog endpoints for public listing and owner management.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    /// <summary>
    /// Returns published blog posts.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BlogPostDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BlogPostDto>>>> GetPublished(
        CancellationToken cancellationToken)
    {
        var result = await _blogService.GetPublishedAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BlogPostDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns a published blog post by slug.
    /// </summary>
    [HttpGet("public/{slug}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogPostDto>>> GetPublishedBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var result = await _blogService.GetPublishedBySlugAsync(slug, cancellationToken);
        return Ok(ApiResponse<BlogPostDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Returns the current user's blog posts.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<BlogPostDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BlogPostDto>>>> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await _blogService.GetMineAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BlogPostDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Creates a blog post for the current user.
    /// </summary>
    [HttpPost("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<BlogPostDto>>> CreateMine(
        [FromBody] CreateBlogPostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _blogService.CreateMineAsync(request, cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<BlogPostDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Updates one of the current user's blog posts.
    /// </summary>
    [HttpPut("me/{postId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogPostDto>>> UpdateMine(
        Guid postId,
        [FromBody] UpdateBlogPostRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _blogService.UpdateMineAsync(postId, request, cancellationToken);
        return Ok(ApiResponse<BlogPostDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Deletes one of the current user's blog posts.
    /// </summary>
    [HttpDelete("me/{postId:guid}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMine(Guid postId, CancellationToken cancellationToken)
    {
        await _blogService.DeleteMineAsync(postId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Publishes one of the current user's blog posts.
    /// </summary>
    [HttpPost("me/{postId:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BlogPostDto>>> PublishMine(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await _blogService.PublishMineAsync(postId, cancellationToken);
        return Ok(ApiResponse<BlogPostDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Unpublishes one of the current user's blog posts.
    /// </summary>
    [HttpPost("me/{postId:guid}/unpublish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<BlogPostDto>>> UnpublishMine(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await _blogService.UnpublishMineAsync(postId, cancellationToken);
        return Ok(ApiResponse<BlogPostDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Uploads or replaces the cover image for one of the current user's blog posts.
    /// </summary>
    [HttpPost("me/{postId:guid}/cover")]
    [Authorize]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogPostDto>>> UploadCover(
        Guid postId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<object>.Fail(
                "Cover file is required.",
                traceId: HttpContext.TraceIdentifier));
        }

        await using var stream = file.OpenReadStream();
        var result = await _blogService.UploadCoverAsync(
            postId,
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            cancellationToken);

        return Ok(ApiResponse<BlogPostDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    /// <summary>
    /// Removes the cover image for one of the current user's blog posts.
    /// </summary>
    [HttpDelete("me/{postId:guid}/cover")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogPostDto>>> RemoveCover(
        Guid postId,
        CancellationToken cancellationToken)
    {
        var result = await _blogService.RemoveCoverAsync(postId, cancellationToken);
        return Ok(ApiResponse<BlogPostDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }
}

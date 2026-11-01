using PersonalBrand.Application.Blog.Contracts;

namespace PersonalBrand.Application.Blog;

/// <summary>
/// Blog post use cases.
/// </summary>
public interface IBlogService
{
    Task<IReadOnlyList<BlogPostDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<BlogPostDto> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BlogPostDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<BlogPostDto> CreateMineAsync(CreateBlogPostRequest request, CancellationToken cancellationToken = default);

    Task<BlogPostDto> UpdateMineAsync(Guid postId, UpdateBlogPostRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid postId, CancellationToken cancellationToken = default);

    Task<BlogPostDto> PublishMineAsync(Guid postId, CancellationToken cancellationToken = default);

    Task<BlogPostDto> UnpublishMineAsync(Guid postId, CancellationToken cancellationToken = default);

    Task<BlogPostDto> UploadCoverAsync(
        Guid postId,
        Stream content,
        string fileName,
        string contentType,
        long? contentLength,
        CancellationToken cancellationToken = default);

    Task<BlogPostDto> RemoveCoverAsync(Guid postId, CancellationToken cancellationToken = default);
}

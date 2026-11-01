namespace PersonalBrand.Application.Blog.Contracts;

/// <summary>
/// Public representation of a blog post.
/// </summary>
public sealed class BlogPostDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Body { get; init; }

    public string? CoverImagePath { get; init; }

    public string? CoverImageUrl { get; init; }

    public string? Tags { get; init; }

    public required bool IsFeatured { get; init; }

    public required bool IsPublished { get; init; }

    public DateTimeOffset? PublishedAtUtc { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

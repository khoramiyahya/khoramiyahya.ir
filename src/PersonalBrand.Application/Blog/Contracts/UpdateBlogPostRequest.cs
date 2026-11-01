namespace PersonalBrand.Application.Blog.Contracts;

/// <summary>
/// Request payload for updating a blog post.
/// </summary>
public sealed class UpdateBlogPostRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? Summary { get; set; }

    public string? Body { get; set; }

    public string? Tags { get; set; }

    public bool IsFeatured { get; set; }

    public bool? IsPublished { get; set; }
}

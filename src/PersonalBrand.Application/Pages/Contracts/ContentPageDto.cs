namespace PersonalBrand.Application.Pages.Contracts;

/// <summary>
/// Public representation of a content page.
/// </summary>
public sealed class ContentPageDto
{
    public required Guid Id { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Body { get; init; }

    public string? MetaTitle { get; init; }

    public string? MetaDescription { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsPublished { get; init; }

    public DateTimeOffset? PublishedAtUtc { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

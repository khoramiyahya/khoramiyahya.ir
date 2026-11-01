namespace PersonalBrand.Application.Search.Contracts;

/// <summary>
/// A single hit from the public site search.
/// </summary>
public sealed class SearchResultItemDto
{
    public required Guid Id { get; init; }

    public required string ContentType { get; init; }

    public required string Title { get; init; }

    public string? Summary { get; init; }

    public string? Slug { get; init; }

    public string? UrlHint { get; init; }

    public required DateTimeOffset UpdatedAtUtc { get; init; }
}

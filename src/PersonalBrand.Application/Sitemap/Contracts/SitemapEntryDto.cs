namespace PersonalBrand.Application.Sitemap.Contracts;

/// <summary>
/// A single public URL entry for sitemap consumers.
/// </summary>
public sealed class SitemapEntryDto
{
    public required string Loc { get; init; }

    public required DateTimeOffset LastModifiedUtc { get; init; }

    public required string ChangeFrequency { get; init; }

    public required decimal Priority { get; init; }

    public required string ContentType { get; init; }
}

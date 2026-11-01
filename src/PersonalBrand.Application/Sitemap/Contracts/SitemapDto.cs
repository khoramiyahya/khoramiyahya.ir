namespace PersonalBrand.Application.Sitemap.Contracts;

/// <summary>
/// Aggregated public sitemap payload.
/// </summary>
public sealed class SitemapDto
{
    public string? BaseUrl { get; init; }

    public required DateTimeOffset GeneratedAtUtc { get; init; }

    public required int TotalCount { get; init; }

    public required IReadOnlyList<SitemapEntryDto> Entries { get; init; }
}

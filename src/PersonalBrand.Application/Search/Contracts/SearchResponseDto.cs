namespace PersonalBrand.Application.Search.Contracts;

/// <summary>
/// Aggregated public search response.
/// </summary>
public sealed class SearchResponseDto
{
    public required string Query { get; init; }

    public required int TotalCount { get; init; }

    public required IReadOnlyList<SearchResultItemDto> Items { get; init; }
}

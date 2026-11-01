using PersonalBrand.Application.Search.Contracts;

namespace PersonalBrand.Application.Search;

/// <summary>
/// Public site-wide search use cases.
/// </summary>
public interface ISearchService
{
    Task<SearchResponseDto> SearchPublishedAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default);
}

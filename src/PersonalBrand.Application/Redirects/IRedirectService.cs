using PersonalBrand.Application.Redirects.Contracts;

namespace PersonalBrand.Application.Redirects;

/// <summary>
/// SEO redirect use cases.
/// </summary>
public interface IRedirectService
{
    Task<IReadOnlyList<SeoRedirectDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<SeoRedirectDto> ResolvePublishedByPathAsync(string fromPath, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SeoRedirectDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<SeoRedirectDto> CreateMineAsync(CreateSeoRedirectRequest request, CancellationToken cancellationToken = default);

    Task<SeoRedirectDto> UpdateMineAsync(
        Guid redirectId,
        UpdateSeoRedirectRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid redirectId, CancellationToken cancellationToken = default);

    Task<SeoRedirectDto> PublishMineAsync(Guid redirectId, CancellationToken cancellationToken = default);

    Task<SeoRedirectDto> UnpublishMineAsync(Guid redirectId, CancellationToken cancellationToken = default);
}

using PersonalBrand.Application.Sitemap.Contracts;

namespace PersonalBrand.Application.Sitemap;

/// <summary>
/// Public sitemap use cases.
/// </summary>
public interface ISitemapService
{
    Task<SitemapDto> GetPublicAsync(CancellationToken cancellationToken = default);

    Task<string> GetPublicXmlAsync(CancellationToken cancellationToken = default);
}

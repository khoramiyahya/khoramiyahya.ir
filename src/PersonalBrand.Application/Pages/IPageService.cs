using PersonalBrand.Application.Pages.Contracts;

namespace PersonalBrand.Application.Pages;

/// <summary>
/// Content page use cases.
/// </summary>
public interface IPageService
{
    Task<IReadOnlyList<ContentPageDto>> GetPublishedAsync(CancellationToken cancellationToken = default);

    Task<ContentPageDto> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContentPageDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<ContentPageDto> CreateMineAsync(CreateContentPageRequest request, CancellationToken cancellationToken = default);

    Task<ContentPageDto> UpdateMineAsync(Guid pageId, UpdateContentPageRequest request, CancellationToken cancellationToken = default);

    Task DeleteMineAsync(Guid pageId, CancellationToken cancellationToken = default);

    Task<ContentPageDto> PublishMineAsync(Guid pageId, CancellationToken cancellationToken = default);

    Task<ContentPageDto> UnpublishMineAsync(Guid pageId, CancellationToken cancellationToken = default);
}
